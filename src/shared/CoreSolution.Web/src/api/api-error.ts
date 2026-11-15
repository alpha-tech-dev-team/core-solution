export class ApiError extends Error {
  readonly status: number | null
  readonly code: string | null

  constructor(status: number | null, code: string | null = null) {
    super(code ?? (status === null ? 'network' : String(status)))
    this.name = ApiError.name
    this.status = status
    this.code = code
  }
}
