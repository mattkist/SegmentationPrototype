import type { ApiErrorBody } from './types'

export class ApiRequestError extends Error {
  status: number
  body: unknown

  constructor(message: string, status: number, body: unknown) {
    super(message)
    this.status = status
    this.body = body
  }
}

async function parseJsonSafe(res: Response): Promise<unknown> {
  const text = await res.text()
  if (!text) return null
  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(path, { headers: { Accept: 'application/json' } })
  const body = await parseJsonSafe(res)
  if (!res.ok) {
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Request failed', res.status, body)
  }
  return body as T
}

export async function apiPost<T>(path: string, json?: unknown): Promise<T> {
  const res = await fetch(path, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: json === undefined ? undefined : JSON.stringify(json),
  })
  const body = await parseJsonSafe(res)
  if (!res.ok) {
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Request failed', res.status, body)
  }
  return body as T
}

export async function apiPut<T>(path: string, json: unknown): Promise<T> {
  const res = await fetch(path, {
    method: 'PUT',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(json),
  })
  const body = await parseJsonSafe(res)
  if (!res.ok) {
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Request failed', res.status, body)
  }
  return body as T
}

export async function apiPatch<T>(path: string, json: unknown): Promise<T> {
  const res = await fetch(path, {
    method: 'PATCH',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(json),
  })
  const body = await parseJsonSafe(res)
  if (!res.ok) {
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Request failed', res.status, body)
  }
  return body as T
}

export async function apiPutEmpty(path: string, json: unknown): Promise<void> {
  const res = await fetch(path, {
    method: 'PUT',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(json),
  })
  if (!res.ok) {
    const body = await parseJsonSafe(res)
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Request failed', res.status, body)
  }
}

export async function apiPostEmpty(path: string): Promise<void> {
  const res = await fetch(path, {
    method: 'POST',
    headers: { Accept: 'application/json' },
  })
  if (!res.ok) {
    const body = await parseJsonSafe(res)
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Request failed', res.status, body)
  }
}

export async function apiPostFormImport(
  path: string,
  file: File,
): Promise<unknown> {
  const fd = new FormData()
  fd.append('file', file)
  const res = await fetch(path, {
    method: 'POST',
    body: fd,
  })
  const body = await parseJsonSafe(res)
  if (!res.ok) {
    const msg =
      typeof body === 'object' && body !== null && 'message' in body
        ? String((body as ApiErrorBody).message)
        : res.statusText
    throw new ApiRequestError(msg || 'Import failed', res.status, body)
  }
  return body
}
