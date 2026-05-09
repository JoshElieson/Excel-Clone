export type CellPayload = {
  contents: string
  value: string
}

export type Snapshot = {
  changed: boolean
  cells: Record<string, CellPayload>
}

const jsonHeaders = { 'Content-Type': 'application/json' }

export async function createSession(): Promise<string> {
  const r = await fetch('/api/sessions', { method: 'POST' })
  if (!r.ok) throw new Error('Could not create session')
  const data = (await r.json()) as { sessionId: string }
  return data.sessionId
}

export async function getSnapshot(sessionId: string): Promise<Snapshot> {
  const r = await fetch(`/api/sessions/${sessionId}/snapshot`)
  if (!r.ok) throw new Error('Could not load snapshot')
  return r.json() as Promise<Snapshot>
}

export async function commitCell(
  sessionId: string,
  cellName: string,
  content: string,
): Promise<Snapshot> {
  const r = await fetch(
    `/api/sessions/${sessionId}/cells/${encodeURIComponent(cellName)}/commit`,
    { method: 'POST', headers: jsonHeaders, body: JSON.stringify({ content }) },
  )
  const body = await r.json().catch(() => ({}))
  if (!r.ok) {
    const msg = typeof body === 'object' && body && 'message' in body
      ? String((body as { message: string }).message)
      : r.statusText
    throw new Error(msg)
  }
  return body as Snapshot
}

export async function resetSession(sessionId: string): Promise<Snapshot> {
  const r = await fetch(`/api/sessions/${sessionId}/reset`, { method: 'POST' })
  if (!r.ok) throw new Error('Could not reset')
  return r.json() as Promise<Snapshot>
}

export async function loadXml(sessionId: string, xml: string): Promise<Snapshot> {
  const r = await fetch(`/api/sessions/${sessionId}/load`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/xml; charset=utf-8' },
    body: xml,
  })
  const body = await r.json().catch(() => ({}))
  if (!r.ok) {
    const msg = typeof body === 'object' && body && 'message' in body
      ? String((body as { message: string }).message)
      : r.statusText
    throw new Error(msg)
  }
  return body as Snapshot
}

export async function saveSpreadsheet(sessionId: string): Promise<Blob> {
  const r = await fetch(`/api/sessions/${sessionId}/save`)
  if (!r.ok) throw new Error('Could not save')
  return r.blob()
}
