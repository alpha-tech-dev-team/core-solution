<script setup lang="ts">
import { nextTick, onMounted, ref, watch } from 'vue'

const props = defineProps<{
  open: boolean
  title: string
}>()

const emit = defineEmits<{
  close: []
}>()

const dialog = ref<HTMLDialogElement | null>(null)

async function sync(open: boolean): Promise<void> {
  await nextTick()

  const element = dialog.value
  if (element === null) return

  if (open && !element.open) {
    element.showModal()
    return
  }

  if (!open && element.open) {
    element.close()
  }
}

watch(() => props.open, sync)
onMounted(() => sync(props.open))
</script>

<template>
  <dialog
    ref="dialog"
    class="modal"
    :aria-label="title"
    @close="emit('close')"
    @click.self="emit('close')"
  >
    <div class="modal__content">
      <h2 class="modal__title">{{ title }}</h2>
      <slot />
    </div>
  </dialog>
</template>

<style scoped>
.modal {
  width: 100%;
  max-width: var(--size-form);
  max-height: 90vh;
  margin: auto;
  padding: 0;
  overflow: auto;
  background: var(--color-surface);
  color: var(--color-text);
  border: var(--border-width) solid var(--color-border);
  border-radius: var(--radius);
}

.modal::backdrop {
  background: var(--color-overlay);
}

.modal__content {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  padding: var(--space-5);
}

.modal__title {
  font-size: var(--font-size-large);
}
</style>
