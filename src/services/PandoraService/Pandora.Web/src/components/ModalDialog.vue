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

<style scoped src="./modal-dialog.css"></style>
