import { useCallback, useEffect, useRef, useState, type CSSProperties } from 'react'
import './App.css'
import {
  type CellPayload,
  type Snapshot,
  commitCell,
  createSession,
  getSnapshot,
  loadXml,
  resetSession,
  saveSpreadsheet,
} from './api'

const COLS = Array.from({ length: 26 }, (_, i) => String.fromCharCode(65 + i))
const ROWS = Array.from({ length: 99 }, (_, i) => i + 1)

const PEN_OPTIONS: { label: string; value: string }[] = [
  { label: 'Black', value: '#000000' },
  { label: 'Red', value: '#ff0000' },
  { label: 'Green', value: '#00c800' },
  { label: 'Blue', value: '#0000ff' },
  { label: 'Purple', value: '#a020f0' },
  { label: 'Yellow', value: '#ffa500' },
]

function cellBelow(name: string): string | null {
  const col = name[0]
  const row = Number.parseInt(name.slice(1), 10)
  if (Number.isNaN(row) || row >= 99) return null
  return `${col}${row + 1}`
}

export default function App() {
  const [sessionId, setSessionId] = useState<string | null>(null)
  const [cells, setCells] = useState<Record<string, CellPayload>>({})
  const [changed, setChanged] = useState(false)
  const [selectedCell, setSelectedCell] = useState('A1')
  const [focus, setFocus] = useState<'grid' | 'bar' | null>(null)
  const [draftText, setDraftText] = useState('')
  const [penColor, setPenColor] = useState(PEN_OPTIONS[0].value)
  const [cellColors, setCellColors] = useState<Record<string, string>>({})
  const [helpOpen, setHelpOpen] = useState(false)

  const inputRefs = useRef<Record<string, HTMLInputElement | null>>({})
  const fileInputRef = useRef<HTMLInputElement | null>(null)

  const applySnapshot = useCallback((snap: Snapshot) => {
    setCells(snap.cells)
    setChanged(snap.changed)
  }, [])

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        const id = await createSession()
        if (cancelled) return
        setSessionId(id)
        const snap = await getSnapshot(id)
        if (cancelled) return
        applySnapshot(snap)
      } catch {
        if (!cancelled) alert('Could not connect to the API. Is SpreadsheetApi running on port 5288?')
      }
    })()
    return () => {
      cancelled = true
    }
  }, [applySnapshot])

  const commitAndGoDown = useCallback(
    async (name: string, raw: string) => {
      if (!sessionId) return
      try {
        const snap = await commitCell(sessionId, name, raw)
        applySnapshot(snap)
        setCellColors((prev) => ({ ...prev, [name]: penColor }))
        const next = cellBelow(name)
        if (next) {
          setSelectedCell(next)
          setFocus('grid')
          const contents = snap.cells[next]?.contents ?? ''
          setDraftText(contents)
          queueMicrotask(() => inputRefs.current[next]?.focus())
        } else {
          setFocus(null)
        }
      } catch (e) {
        alert(e instanceof Error ? e.message : 'Error')
      }
    },
    [sessionId, applySnapshot, penColor],
  )

  const displayForCell = (name: string) => {
    if (selectedCell === name && focus !== null) return draftText
    return cells[name]?.value ?? ''
  }

  const formulaBarValue =
    focus !== null ? draftText : (cells[selectedCell]?.contents ?? '')

  const onNew = async () => {
    if (!sessionId) return
    if (changed && !window.confirm('Discard unsaved changes and start a new spreadsheet?')) return
    try {
      const snap = await resetSession(sessionId)
      applySnapshot(snap)
      setCellColors({})
      setSelectedCell('A1')
      setFocus(null)
      setDraftText('')
      queueMicrotask(() => inputRefs.current.A1?.focus())
    } catch {
      alert('Could not reset.')
    }
  }

  const onOpenClick = () => {
    if (changed && !window.confirm('You have unsaved changes. Replace the current spreadsheet?')) return
    fileInputRef.current?.click()
  }

  const onFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    e.target.value = ''
    if (!f || !sessionId) return
    try {
      const text = await f.text()
      const snap = await loadXml(sessionId, text)
      applySnapshot(snap)
      setCellColors({})
      setSelectedCell('A1')
      setFocus(null)
      setDraftText('')
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Open failed')
    }
  }

  const onSave = async () => {
    if (!sessionId) return
    try {
      const blob = await saveSpreadsheet(sessionId)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'spreadsheet.sprd'
      a.click()
      URL.revokeObjectURL(url)
      const snap = await getSnapshot(sessionId)
      applySnapshot(snap)
    } catch {
      alert('Save failed')
    }
  }

  if (!sessionId) {
    return (
      <div className="app-shell">
        <p className="status">Connecting to spreadsheet API…</p>
      </div>
    )
  }

  return (
    <div className="app-shell">
      <header className="toolbar">
        <h1>Spreadsheet</h1>
        <div className="toolbar-actions">
          <button type="button" onClick={onNew}>
            New
          </button>
          <button type="button" onClick={onOpenClick}>
            Open
          </button>
          <button type="button" onClick={onSave}>
            Save
          </button>
          <button type="button" onClick={() => setHelpOpen(true)}>
            Help
          </button>
        </div>
        <div
          className="pen-picker"
          role="group"
          aria-label="Text color for new edits"
        >
          <span className="pen-picker-label">Text</span>
          <div className="pen-swatches">
            {PEN_OPTIONS.map((o) => (
              <button
                key={o.value}
                type="button"
                className={`pen-swatch${penColor === o.value ? ' pen-swatch--active' : ''}`}
                style={{ '--swatch': o.value } as CSSProperties}
                onClick={() => setPenColor(o.value)}
                title={o.label}
                aria-label={o.label}
                aria-pressed={penColor === o.value}
              />
            ))}
          </div>
        </div>
      </header>

      <input
        ref={fileInputRef}
        type="file"
        accept=".sprd,.xml,text/xml,application/xml"
        className="hidden-file"
        onChange={onFile}
      />

      <div className="formula-panel">
        <label>
          Cell: <strong>{selectedCell}</strong>
        </label>
        <label>
          Contents
          <input
            type="text"
            data-formula-bar="1"
            autoComplete="off"
            value={formulaBarValue}
            onMouseDown={(e) => e.preventDefault()}
            onFocus={() => {
              setFocus('bar')
              setDraftText(cells[selectedCell]?.contents ?? '')
            }}
            onChange={(e) => {
              setDraftText(e.target.value)
              setCellColors((p) => ({ ...p, [selectedCell]: penColor }))
            }}
            onBlur={(e) => {
              const t = e.relatedTarget as HTMLElement | null
              if (t?.dataset.cell === selectedCell) return
              setFocus(null)
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault()
                void commitAndGoDown(selectedCell, e.currentTarget.value)
              }
            }}
          />
        </label>
        <label>
          Value: <strong>{cells[selectedCell]?.value ?? ''}</strong>
        </label>
      </div>

      <div className="sheet-scroll">
        <table className="sheet">
          <thead>
            <tr>
              <th aria-label="corner" />
              {COLS.map((c) => (
                <th key={c}>{c}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {ROWS.map((row) => (
              <tr key={row}>
                <th scope="row">{row}</th>
                {COLS.map((col) => {
                  const name = `${col}${row}`
                  return (
                    <td key={name}>
                      <input
                        ref={(el) => {
                          inputRefs.current[name] = el
                        }}
                        className="cell-input"
                        type="text"
                        data-cell={name}
                        autoComplete="off"
                        value={displayForCell(name)}
                        style={{ color: cellColors[name] ?? '#111' }}
                        onFocus={() => {
                          setSelectedCell(name)
                          setFocus('grid')
                          setDraftText(cells[name]?.contents ?? '')
                        }}
                        onChange={(e) => {
                          setDraftText(e.target.value)
                          setCellColors((p) => ({ ...p, [name]: penColor }))
                        }}
                        onBlur={(e) => {
                          const t = e.relatedTarget as HTMLElement | null
                          if (t?.dataset.formulaBar === '1') {
                            setFocus('bar')
                            setDraftText(e.target.value)
                            return
                          }
                          setFocus(null)
                        }}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter') {
                            e.preventDefault()
                            void commitAndGoDown(name, e.currentTarget.value)
                          }
                        }}
                      />
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <p className="status">
        {changed ? 'Unsaved changes' : 'All changes saved'} · Session{' '}
        <code>{sessionId.slice(0, 8)}…</code>
      </p>

      {helpOpen && (
        <div
          className="modal-backdrop"
          role="presentation"
          onClick={() => setHelpOpen(false)}
        >
          <div
            className="modal"
            role="dialog"
            aria-labelledby="help-title"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="help-title">How to use this spreadsheet</h2>
            <p>
              Each cell can hold a number, text, or a formula (start with <code>=</code>).
              Formulas can reference other cells (for example <code>=A1+1</code>).
            </p>
            <p>
              Press <strong>Enter</strong> in a cell or in the contents bar to commit and move
              down (matching the desktop app). If you click away without Enter, edits in that
              cell are discarded.
            </p>
            <p>
              Use <strong>New</strong>, <strong>Open</strong>, and <strong>Save</strong> for
              XML <code>.sprd</code> files. Choose a text color before you type to color that
              cell&apos;s text after you commit.
            </p>
            <button type="button" onClick={() => setHelpOpen(false)}>
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
