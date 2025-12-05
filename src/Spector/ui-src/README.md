# Spector UI (React + Vite)

This directory contains the React-based UI for the Spector network inspector.

## Development

### Prerequisites
- Node.js 20.19+ or 22.12+ (recommended)
- npm

### Getting Started

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Start development server**:
   ```bash
   npm run dev
   ```
   
   The dev server will start at `http://localhost:5173` and proxy `/spector` requests to your .NET application (assumed to be running on `http://localhost:5000`).

3. **Make sure your .NET application is running**:
   ```bash
   cd ..
   dotnet run
   ```

### Building for Production

```bash
npm run build
```

This will compile the React app and output the production build to `../wwwroot/`.

## Project Structure

```
ui-src/
├── src/
│   ├── components/          # React components
│   │   ├── Header/         # Header with connection status
│   │   ├── Sidebar/        # Filters and statistics
│   │   ├── TraceList/      # Trace groups and activities
│   │   ├── DetailsPanel/   # Activity details panel
│   │   └── common/         # Reusable components
│   ├── context/            # React Context for state management
│   ├── hooks/              # Custom React hooks (SSE, etc.)
│   ├── types/              # TypeScript type definitions
│   ├── utils/              # Utility functions
│   ├── styles/             # Global styles and CSS variables
│   ├── App.tsx             # Main app component
│   └── main.tsx            # Entry point
├── index.html              # HTML template
├── vite.config.ts          # Vite configuration
├── tsconfig.json           # TypeScript configuration
└── package.json            # Dependencies and scripts
```

## Key Features

- **Real-time Updates**: SSE connection with auto-reconnect
- **Component-Based**: Modular React components with TypeScript
- **CSS Modules**: Scoped styling for each component
- **State Management**: React Context for global state
- **Type Safety**: Full TypeScript support

## Development Workflow

1. Make changes to React components in `src/`
2. Vite will hot-reload your changes automatically
3. Test with your .NET application running
4. Build for production when ready

## Build Integration

The React app is automatically built when you build the .NET project:

```bash
cd ..
dotnet build
```

This will:
1. Run `npm install` (if needed)
2. Run `npm run build`
3. Embed the output in the .NET assembly

## Troubleshooting

### Port Conflicts
If port 5173 is already in use, you can change it in `vite.config.ts`:
```typescript
server: {
  port: 3000, // Change to your preferred port
  // ...
}
```

### Proxy Issues
If the SSE endpoint is on a different port, update the proxy target in `vite.config.ts`:
```typescript
proxy: {
  '/spector': {
    target: 'http://localhost:YOUR_PORT',
    changeOrigin: true
  }
}
```

### Node Version Warning
If you see a Node.js version warning, it's safe to ignore. The app will still work with Node.js 20.15.1+, though 20.19+ is recommended.
