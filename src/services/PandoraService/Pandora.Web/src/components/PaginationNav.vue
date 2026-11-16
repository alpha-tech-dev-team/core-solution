<script setup lang="ts">
import { FIRST_PAGE } from '@/constants/pagination'

defineProps<{
  page: number
  totalPages: number
  prevLabel: string
  nextLabel: string
  infoLabel: string
}>()

const emit = defineEmits<{
  change: [page: number]
}>()
</script>

<template>
  <nav v-if="totalPages > FIRST_PAGE" class="pager">
    <button
      class="btn btn--ghost btn--sm"
      :disabled="page <= FIRST_PAGE"
      @click="emit('change', page - 1)"
    >
      {{ prevLabel }}
    </button>
    <span class="pager__label">{{ infoLabel }}</span>
    <button
      class="btn btn--ghost btn--sm"
      :disabled="page >= totalPages"
      @click="emit('change', page + 1)"
    >
      {{ nextLabel }}
    </button>
  </nav>
</template>
