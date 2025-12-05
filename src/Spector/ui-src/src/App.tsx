import { SpectorProvider, useSpector } from './context/SpectorContext';
import { useSSE } from './hooks/useSSE';
import { Header } from './components/Header/Header';
import { Sidebar } from './components/Sidebar/Sidebar';
import { TraceList } from './components/TraceList/TraceList';
import { DetailsPanel } from './components/DetailsPanel/DetailsPanel';
import './styles/variables.css';
import './styles/global.css';

function AppContent() {
  const { addActivity, setConnectionStatus, isPaused } = useSpector();

  useSSE({
    url: '/spector/events',
    onMessage: addActivity,
    onStatusChange: setConnectionStatus,
    isPaused
  });

  return (
    <div className="container">
      <Header />
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <Sidebar />
        <TraceList />
        <DetailsPanel />
      </div>
    </div>
  );
}

function App() {
  return (
    <SpectorProvider>
      <AppContent />
    </SpectorProvider>
  );
}

export default App;
