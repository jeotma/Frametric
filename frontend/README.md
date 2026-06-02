# FrametricFrontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.13.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, run:

```bash
npm run test
# or
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing using **Playwright**, run:

```bash
# Run tests in headless mode
npm run e2e

# Run tests with the Playwright UI runner
npm run e2e:ui
```

## OpenAPI Client Generation

The API client is automatically generated from the backend OpenAPI/Swagger specification. To update the generated client files:

```bash
# 1. Download the latest spec from the running backend
npm run download:spec

# 2. Re-generate the Angular services and model DTOs
npm run generate:api
```

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
