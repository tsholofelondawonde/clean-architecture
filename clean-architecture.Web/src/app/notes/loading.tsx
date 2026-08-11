export default function Loading() {
  return (
    <div className="flex w-full max-w-2xl flex-col gap-10 px-6 py-12">
      <div className="flex flex-col gap-2">
        <div className="h-7 w-24 animate-pulse rounded-md bg-surface" />
        <div className="h-4 w-48 animate-pulse rounded-md bg-surface" />
      </div>

      <div className="flex flex-col gap-4">
        <div className="h-9 animate-pulse rounded-lg bg-surface" />
        <div className="h-24 animate-pulse rounded-lg bg-surface" />
        <div className="h-9 w-28 animate-pulse rounded-lg bg-surface" />
      </div>

      <div className="flex flex-col gap-2">
        {Array.from({ length: 3 }).map((_, index) => (
          <div
            key={index}
            className="h-12 animate-pulse rounded-lg border border-border bg-surface"
          />
        ))}
      </div>
    </div>
  );
}
