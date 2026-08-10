export async function fetchServer<T>(
  path: string,
  init?: RequestInit
): Promise<T | null> {
  const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${path}`, {
    cache: "no-store",
    ...init,
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Request to ${path} failed with status ${response.status}`);
  }

  return (await response.json()) as T;
}
