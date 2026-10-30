import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { StorageKey, StoreId } from '@/constants/auth.Constants'
import type { AuthTokensResponse } from '@/api/contracts'

export const useAuthStore = defineStore(StoreId.Auth, () => {
  const token = ref<string | null>(localStorage.getItem(StorageKey.AuthToken))

  const isAuthenticated = computed(() => !!token.value)

  function setSession(session: AuthTokensResponse) {
    token.value = session.accessToken
    localStorage.setItem(StorageKey.AuthToken, session.accessToken)
  }

  function logout() {
    token.value = null
    localStorage.removeItem(StorageKey.AuthToken)
  }

  return {
    token,
    isAuthenticated,
    setSession,
    logout,
  }
})
