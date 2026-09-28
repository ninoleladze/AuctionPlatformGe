import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./vitest.setup.ts'],
    // Only run our pure-function store tests; Angular TestBed specs use ng test / Karma
    include: ['src/app/store/**/*.spec.ts'],
  },
});
