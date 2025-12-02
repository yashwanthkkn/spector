// State Management
const state = {
    traces: new Map(), // TraceId -> { activities: [], startTime, endTime }
    activities: new Map(), // SpanId -> activity data
    isPaused: false,
    filters: {
        httpIn: true,
        httpOut: true
    },
    selectedActivity: null
};

// DOM Elements
const elements = {
    statusIndicator: document.getElementById('statusIndicator'),
    statusText: document.getElementById('statusText'),
    traceList: document.getElementById('traceList'),
    detailsPanel: document.getElementById('detailsPanel'),
    detailsContent: document.getElementById('detailsContent'),
    clearBtn: document.getElementById('clearBtn'),
    pauseBtn: document.getElementById('pauseBtn'),
    closeDetailsBtn: document.getElementById('closeDetailsBtn'),
    filterHttpIn: document.getElementById('filterHttpIn'),
    filterHttpOut: document.getElementById('filterHttpOut'),
    totalRequests: document.getElementById('totalRequests'),
    activeTraces: document.getElementById('activeTraces')
};

// SSE Connection
let eventSource = null;

function connectSSE() {
    eventSource = new EventSource('/spector/events');

    eventSource.onopen = () => {
        updateConnectionStatus(true);
        console.log('SSE Connected');
    };

    eventSource.onmessage = (event) => {
        if (state.isPaused) return;

        try {
            const activity = JSON.parse(event.data);
            console.log(activity);
            processActivity(activity);
        } catch (error) {
            console.error('Error parsing SSE data:', error);
        }
    };

    eventSource.onerror = (error) => {
        updateConnectionStatus(false);
        console.error('SSE Error:', error);

        // Attempt to reconnect after 3 seconds
        setTimeout(() => {
            if (eventSource.readyState === EventSource.CLOSED) {
                connectSSE();
            }
        }, 3000);
    };
}

function updateConnectionStatus(connected) {
    if (connected) {
        elements.statusIndicator.classList.add('connected');
        elements.statusIndicator.classList.remove('disconnected');
        elements.statusText.textContent = 'Connected';
    } else {
        elements.statusIndicator.classList.remove('connected');
        elements.statusIndicator.classList.add('disconnected');
        elements.statusText.textContent = 'Disconnected';
    }
}

// Activity Processing
function processActivity(activity) {
    const { TraceId, SpanId, ParentSpanId, Name, Tags } = activity;

    // Store activity
    state.activities.set(SpanId, activity);

    // Get or create trace group
    if (!state.traces.has(TraceId)) {
        state.traces.set(TraceId, {
            traceId: TraceId,
            activities: [],
            startTime: new Date(activity.StartTimeUtc),
            endTime: new Date(activity.StartTimeUtc)
        });
    }

    const trace = state.traces.get(TraceId);
    trace.activities.push(activity);

    // Update trace timing
    const activityStart = new Date(activity.StartTimeUtc);
    const activityEnd = new Date(activityStart.getTime() + parseDuration(activity.Duration));

    if (activityStart < trace.startTime) trace.startTime = activityStart;
    if (activityEnd > trace.endTime) trace.endTime = activityEnd;

    // Update UI
    updateStats();
    renderTraces();
}

function parseDuration(duration) {
    // Parse duration string like "00:00:00.3008980" to milliseconds
    const parts = duration.split(':');
    const hours = parseInt(parts[0]);
    const minutes = parseInt(parts[1]);
    const secondsParts = parts[2].split('.');
    const seconds = parseInt(secondsParts[0]);
    const milliseconds = secondsParts[1] ? parseInt(secondsParts[1].substring(0, 3)) : 0;

    return (hours * 3600 + minutes * 60 + seconds) * 1000 + milliseconds;
}

function formatDuration(ms) {
    if (ms < 1) return `${(ms * 1000).toFixed(0)}μs`;
    if (ms < 1000) return `${ms.toFixed(0)}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
}

// Rendering
function renderTraces() {
    const sortedTraces = Array.from(state.traces.values())
        .sort((a, b) => b.startTime - a.startTime);

    if (sortedTraces.length === 0) {
        elements.traceList.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">📡</div>
                <p>Waiting for network activity...</p>
                <small>Make API requests to see them appear here</small>
            </div>
        `;
        return;
    }

    elements.traceList.innerHTML = sortedTraces
        .map(trace => renderTraceGroup(trace))
        .join('');

    // Attach event listeners
    attachTraceEventListeners();
}

function renderTraceGroup(trace) {
    const totalDuration = trace.endTime - trace.startTime;
    const activityCount = trace.activities.length;

    // Build activity hierarchy
    const rootActivities = [];
    const childrenMap = new Map();

    trace.activities.forEach(activity => {
        if (!activity.ParentSpanId || !state.activities.has(activity.ParentSpanId)) {
            rootActivities.push(activity);
        } else {
            if (!childrenMap.has(activity.ParentSpanId)) {
                childrenMap.set(activity.ParentSpanId, []);
            }
            childrenMap.get(activity.ParentSpanId).push(activity);
        }
    });

    const activitiesHtml = rootActivities
        .map(activity => renderActivityItem(activity, childrenMap, trace.startTime, totalDuration))
        .join('');

    return `
        <div class="trace-group" data-trace-id="${trace.traceId}">
            <div class="trace-header" onclick="toggleTrace('${trace.traceId}')">
                <div class="trace-info">
                    <div class="trace-title">Trace</div>
                    <div class="trace-id">${trace.traceId}</div>
                </div>
                <div class="trace-meta">
                    <span class="trace-duration">${formatDuration(totalDuration)}</span>
                    <span class="trace-count">${activityCount} request${activityCount !== 1 ? 's' : ''}</span>
                    <span class="collapse-icon">▼</span>
                </div>
            </div>
            <div class="trace-activities">
                ${activitiesHtml}
            </div>
        </div>
    `;
}

function renderActivityItem(activity, childrenMap, traceStartTime, traceDuration, isChild = false) {
    const type = activity.Name.toLowerCase();
    const method = activity.Tags['spector.method'] || 'N/A';
    const url = activity.Tags['spector.url'] || 'N/A';
    const status = activity.Tags['spector.status'] || '';
    const duration = parseDuration(activity.Duration);

    // Check filters
    if (type === 'httpin' && !state.filters.httpIn) return '';
    if (type === 'httpout' && !state.filters.httpOut) return '';

    // Calculate timeline position and width
    const activityStart = new Date(activity.StartTimeUtc);
    const offset = activityStart - traceStartTime;
    const timelineWidth = traceDuration > 0 ? (duration / traceDuration) * 100 : 100;

    const children = childrenMap.get(activity.SpanId) || [];
    const childrenHtml = children
        .map(child => renderActivityItem(child, childrenMap, traceStartTime, traceDuration, true))
        .join('');

    // Determine status color
    let statusClass = '';
    if (status) {
        const statusCode = parseInt(status);
        if (statusCode >= 200 && statusCode < 300) statusClass = 'status-success';
        else if (statusCode >= 300 && statusCode < 400) statusClass = 'status-redirect';
        else if (statusCode >= 400 && statusCode < 500) statusClass = 'status-client-error';
        else if (statusCode >= 500) statusClass = 'status-server-error';
    }

    return `
        <div class="activity-item ${isChild ? 'child' : ''}" 
             data-span-id="${activity.SpanId}"
             onclick="selectActivity('${activity.SpanId}', event)">
            <span class="activity-type ${type}"></span>
            <div class="activity-content">
                <div class="activity-name">
                    <span class="activity-method method-${method}">${method}</span>
                    ${status ? `<span class="activity-status ${statusClass}">${status}</span>` : ''}
                </div>
                <div class="activity-url">${url}</div>
            </div>
            <div class="activity-timing">
                <span class="activity-duration">${formatDuration(duration)}</span>
                <div class="activity-timeline">
                    <div class="timeline-bar" style="width: ${timelineWidth}%"></div>
                </div>
            </div>
        </div>
        ${childrenHtml}
    `;
}

function attachTraceEventListeners() {
    // Event listeners are attached via onclick in HTML for simplicity
}

// Event Handlers
function toggleTrace(traceId) {
    const traceGroup = document.querySelector(`[data-trace-id="${traceId}"]`);
    const header = traceGroup.querySelector('.trace-header');
    const activities = traceGroup.querySelector('.trace-activities');
    const icon = header.querySelector('.collapse-icon');

    if (activities.style.display === 'none') {
        activities.style.display = 'block';
        icon.classList.remove('collapsed');
        header.classList.remove('collapsed');
    } else {
        activities.style.display = 'none';
        icon.classList.add('collapsed');
        header.classList.add('collapsed');
    }
}

function selectActivity(spanId, event) {
    event.stopPropagation();

    // Remove previous selection
    document.querySelectorAll('.activity-item.selected').forEach(el => {
        el.classList.remove('selected');
    });

    // Add new selection
    const activityElement = document.querySelector(`[data-span-id="${spanId}"]`);
    activityElement.classList.add('selected');

    state.selectedActivity = state.activities.get(spanId);
    renderActivityDetails(state.selectedActivity);
    elements.detailsPanel.classList.add('open');
}

function renderActivityDetails(activity) {
    const type = activity.Name.toLowerCase();
    const method = activity.Tags['spector.method'] || 'N/A';
    const url = activity.Tags['spector.url'] || 'N/A';
    const duration = parseDuration(activity.Duration);
    const requestBody = activity.Tags['spector.requestBody'] || '';
    const responseBody = activity.Tags['spector.responseBody'] || '';

    let requestBodyHtml = '';
    let responseBodyHtml = '';

    if (requestBody) {
        const formatted = formatJson(requestBody);
        requestBodyHtml = `
            <div class="detail-section">
                <h4>Request Body</h4>
                <div class="code-block"><pre>${escapeHtml(formatted)}</pre></div>
            </div>
        `;
    }

    if (responseBody) {
        const formatted = formatJson(responseBody);
        responseBodyHtml = `
            <div class="detail-section">
                <h4>Response Body</h4>
                <div class="code-block"><pre>${escapeHtml(formatted)}</pre></div>
            </div>
        `;
    }

    elements.detailsContent.innerHTML = `
        <div class="detail-section">
            <h4>Overview</h4>
            <div class="detail-row">
                <span class="detail-label">Type</span>
                <span class="detail-value"><span class="badge ${type}">${activity.Name}</span></span>
            </div>
            <div class="detail-row">
                <span class="detail-label">Method</span>
                <span class="detail-value"><span class="activity-method method-${method}">${method}</span></span>
            </div>
            <div class="detail-row">
                <span class="detail-label">URL</span>
                <span class="detail-value">${escapeHtml(url)}</span>
            </div>
            <div class="detail-row">
                <span class="detail-label">Duration</span>
                <span class="detail-value">${formatDuration(duration)}</span>
            </div>
        </div>
        
        <div class="detail-section">
            <h4>Timing</h4>
            <div class="detail-row">
                <span class="detail-label">Start Time</span>
                <span class="detail-value">${new Date(activity.StartTimeUtc).toLocaleString()}</span>
            </div>
            <div class="detail-row">
                <span class="detail-label">Duration</span>
                <span class="detail-value">${activity.Duration}</span>
            </div>
        </div>
        
        ${requestBodyHtml}
        ${responseBodyHtml}
    `;
}

function formatJson(jsonString) {
    if (!jsonString || jsonString.trim() === '') {
        return '';
    }

    try {
        // First, try to parse the string as JSON
        let parsed = JSON.parse(jsonString);

        // Check if it's a wrapper object with 'content' field
        if (parsed && typeof parsed === 'object' && parsed.content !== undefined) {
            // If content is a string, try to parse it as JSON
            if (typeof parsed.content === 'string') {
                try {
                    const innerParsed = JSON.parse(parsed.content);
                    // Use the inner parsed content instead
                    parsed = innerParsed;
                } catch {
                    // If parsing fails, use the content string as-is
                    return parsed.content;
                }
            } else {
                // If content is already an object, use it
                parsed = parsed.content;
            }
        }

        // Recursively unwrap if there are more nested JSON strings
        if (typeof parsed === 'string') {
            try {
                const innerParsed = JSON.parse(parsed);
                parsed = innerParsed;
            } catch {
                // Not JSON, return as-is
                return parsed;
            }
        }

        // Format the final parsed object
        return JSON.stringify(parsed, null, 2);
    } catch (error) {
        // If parsing fails, return the original string
        return jsonString;
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function updateStats() {
    const totalActivities = state.activities.size;
    const totalTraces = state.traces.size;

    elements.totalRequests.textContent = totalActivities;
    elements.activeTraces.textContent = totalTraces;
}

// Button Handlers
elements.clearBtn.addEventListener('click', () => {
    state.traces.clear();
    state.activities.clear();
    state.selectedActivity = null;
    elements.detailsPanel.classList.remove('open');
    updateStats();
    renderTraces();
});

elements.pauseBtn.addEventListener('click', () => {
    state.isPaused = !state.isPaused;
    elements.pauseBtn.textContent = state.isPaused ? 'Resume' : 'Pause';
    elements.pauseBtn.style.background = state.isPaused ? 'var(--accent-orange)' : '';
});

elements.closeDetailsBtn.addEventListener('click', () => {
    elements.detailsPanel.classList.remove('open');
    document.querySelectorAll('.activity-item.selected').forEach(el => {
        el.classList.remove('selected');
    });
});

elements.filterHttpIn.addEventListener('change', (e) => {
    state.filters.httpIn = e.target.checked;
    renderTraces();
});

elements.filterHttpOut.addEventListener('change', (e) => {
    state.filters.httpOut = e.target.checked;
    renderTraces();
});

// Initialize
connectSSE();
updateStats();
