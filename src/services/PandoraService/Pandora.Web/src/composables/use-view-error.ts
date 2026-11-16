import { ref, type Ref } from 'vue'
import { useTranslations } from '@shared/i18n/create-i18n'

export interface ViewErrorState {
  errorMessage: Ref<string>
  clearError(): void
  run(action: () => Promise<void>): Promise<boolean>
}

export function useViewError(): ViewErrorState {
  const { trError } = useTranslations()
  const errorMessage = ref('')

  return {
    errorMessage,
    clearError: () => {
      errorMessage.value = ''
    },
    run: async (action) => {
      errorMessage.value = ''

      try {
        await action()
        return true
      } catch (error) {
        errorMessage.value = trError(error)
        return false
      }
    },
  }
}
