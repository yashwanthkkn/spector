import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  base: './', // Use relative paths for assets
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'assets/[name]-src.js',
        chunkFileNames: 'assets/[name]-src.js',
        assetFileNames: 'assets/[name]-src.[ext]'
      }
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/spector': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
})
