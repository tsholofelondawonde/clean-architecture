# CLAUDE.md - Frontend

## Tech Stack
- Next.js (App Router) + React 19 + TypeScript
- Axios for client-side HTTP; native `fetch` for Server Components (see Data Fetching)
- TailwindCSS 4.x for styling
- @heroicons/react for icons
- Zustand for client-side state management

## Rendering Model (fill in — do not skip)
- Default: Server Components unless a file needs interactivity, hooks, or browser APIs.
- Mark Client Components explicitly with `'use client'` at the top of the file.
- State the rule for where each feature's data fetching happens: server-side (`fetch`, no interceptor) vs client-side (Axios, needs interceptor/localStorage/cookies).

## Architecture
- Vertical Slice Architecture: organize by feature, not by type.
- Each feature: page component, API service, types, child components.
- Routing bridge: `app/[route]/page.tsx` is a thin file that imports and renders the page component from `src/features/[feature-name]/`. Route params/segment config live in `app/`; feature logic lives in `src/features/`.

## Project Structure
- `src/features/[feature-name]/` — Page component, components, service, types
- `src/shared/components/` — Reusable UI components
- `src/shared/hooks/` — Custom hooks (useAuth, useApi)
- `src/shared/lib/api.ts` — Axios instance with interceptors (client-side only)
- `src/shared/lib/fetch-server.ts` — Server-side fetch wrapper (state whether this exists / is needed)
- `src/shared/types/` — Shared TypeScript types

## Data Fetching
- Server Components: use `fetch` directly, leverage Next.js caching/dedup. No Axios.
- Client Components: use the typed per-feature Axios service. Never raw Axios calls inside components.
- Auth token source of truth: state explicitly (cookie vs localStorage) — this determines whether Server Components can read auth state at all.

## Conventions
- Env vars: `NEXT_PUBLIC_API_URL` for anything read in the browser; unprefixed vars are server-only and will be `undefined` client-side. Confirm current `.env` matches this before using it.
- Typed API service per feature (never raw Axios calls in components).
- Axios interceptor for 401 → refresh token flow (client-side only — specify the server-side equivalent, if any, e.g. middleware-based refresh).
- Conditional rendering based on HATEOAS links: hide or disable UI actions when the corresponding `_links` entry is absent from the API response, rather than hardcoding permission checks.
- Named exports only (no default exports), except where Next.js requires a default export (`page.tsx`, `layout.tsx`, `loading.tsx`, `error.tsx`).

## File Conventions
- `loading.tsx` / `error.tsx` per route segment where relevant — state your policy (every route vs only slow/error-prone ones).
- Images via `next/image`, not raw `<img>`.
- Metadata via the Metadata API (`generateMetadata` or static `metadata` export), not manual `<head>` tags.

## Patterns We Do NOT Use
- Redux or React context + hooks
- CSS modules (use TailwindCSS)
- Default exports (except Next.js special files, see above)
- Client-side data fetching for anything that can be a Server Component