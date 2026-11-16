import { createAppSession } from '@shared/app/create-app-session'

export const appSession = createAppSession()

export const useAuthStore = appSession.session.useAuthStore
export const { api } = appSession
