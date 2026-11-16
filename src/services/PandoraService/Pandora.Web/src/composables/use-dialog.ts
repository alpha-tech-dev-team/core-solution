import { ref, type Ref } from 'vue'

export interface DialogState<T> {
  isOpen: Ref<boolean>
  payload: Ref<T | null>
  open(payload?: T): void
  close(): void
}

export function useDialog<T = undefined>(): DialogState<T> {
  const isOpen = ref(false)
  const payload = ref<T | null>(null) as Ref<T | null>

  return {
    isOpen,
    payload,
    open: (next?: T) => {
      payload.value = (next ?? null) as T | null
      isOpen.value = true
    },
    close: () => {
      isOpen.value = false
      payload.value = null
    },
  }
}
