import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

// In development, requests to /api are forwarded to the ASP.NET Core API.
export default defineConfig({
  plugins: [react()],
  server: { proxy: { "/api": "http://localhost:5080" } },
});
