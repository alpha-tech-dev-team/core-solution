<script setup lang="ts">
import { RESOURCE_KEYS } from '@/constants/i18n'
import { tr } from '@/i18n'
import { formatEmployeeFullName } from '@/models/employee'
import type { Employee } from '@/models/employee'
import ModalDialog from '@/components/ModalDialog.vue'

defineProps<{
  open: boolean
  employee: Employee | null
  isSelf: boolean
  error: string
}>()

const emit = defineEmits<{
  confirm: []
  close: []
}>()
</script>

<template>
  <ModalDialog :open="open" :title="tr(RESOURCE_KEYS.EMPLOYEE_DELETE_TITLE)" @close="emit('close')">
    <p v-if="employee" class="dialog__question">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE_QUESTION, formatEmployeeFullName(employee)) }}
    </p>
    <p v-if="isSelf" class="dialog__warning">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE_SELF_WARNING) }}
    </p>

    <p v-if="error" class="dialog__error">{{ error }}</p>

    <div class="dialog__actions">
      <button type="button" class="dialog__button dialog__button--danger" @click="emit('confirm')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE) }}
      </button>
      <button type="button" class="dialog__button" @click="emit('close')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_CANCEL) }}
      </button>
    </div>
  </ModalDialog>
</template>

<style scoped src="./employee-delete-dialog.css"></style>
