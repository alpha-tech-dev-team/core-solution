export function readJson<T>(key: string): T | null {
  const raw = localStorage.getItem(key)
  if (raw === null) return null

  try {
    return JSON.parse(raw) as T
  } catch {
    localStorage.removeItem(key)
    return null
  }
}

export function writeJson(key: string, value: unknown): void {
  localStorage.setItem(key, JSON.stringify(value))
}

export function removeJson(key: string): void {
  localStorage.removeItem(key)
}
