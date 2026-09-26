const AUTH_TOKEN_STORAGE_KEY = 'smart-solar-web:access-token'

function getStorage(): Storage | null {
  return typeof window === 'undefined' ? null : window.sessionStorage
}

export const authSession = {
  getToken(): string | null {
    try {
      return getStorage()?.getItem(AUTH_TOKEN_STORAGE_KEY) ?? null
    } catch {
      return null
    }
  },

  setToken(token: string): boolean {
    try {
      const storage = getStorage()
      if (!storage) return false
      storage.setItem(AUTH_TOKEN_STORAGE_KEY, token)
      return true
    } catch {
      return false
    }
  },

  clearToken(): void {
    try {
      getStorage()?.removeItem(AUTH_TOKEN_STORAGE_KEY)
    } catch {
      // An inaccessible browser store already behaves as an empty session.
    }
  },
}
