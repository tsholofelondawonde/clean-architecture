"use client";

type ErrorPageProps = {
  error: Error;
  reset: () => void;
};

export default function Error({ error, reset }: ErrorPageProps) {
  return (
    <div className="flex flex-col gap-4 px-6 py-16">
      <p className="text-red-600">Failed to load notes: {error.message}</p>
      <button
        type="button"
        onClick={reset}
        className="self-start rounded bg-foreground px-4 py-2 text-background"
      >
        Try again
      </button>
    </div>
  );
}
