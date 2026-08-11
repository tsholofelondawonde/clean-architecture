"use client";

import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";

type ErrorPageProps = {
  error: Error;
  reset: () => void;
};

export default function Error({ error, reset }: ErrorPageProps) {
  return (
    <div className="flex w-full max-w-2xl flex-col items-center gap-3 px-6 py-16 text-center">
      <ExclamationTriangleIcon className="size-8 text-danger" aria-hidden="true" />
      <div className="flex flex-col gap-1">
        <p className="font-medium">Could not load your notes</p>
        <p className="text-sm text-muted">{error.message}</p>
      </div>
      <button
        type="button"
        onClick={reset}
        className="mt-2 rounded-lg bg-accent px-4 py-2 text-sm font-medium text-accent-foreground transition active:scale-[0.98]"
      >
        Try again
      </button>
    </div>
  );
}
