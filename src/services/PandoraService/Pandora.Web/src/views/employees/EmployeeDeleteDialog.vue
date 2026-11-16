<script setup lang="ts">
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { formatEmployeeFullName } from '@/models/employee'
import type { Employee } from '@/models/employee'
import ModalDialog from '@shared/components/ModalDialog.vue'

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

const { tr } = useTranslations()
</script>

<template>
  <ModalDialog :open="open" :title="tr(RESOURCE_KEYS.EMPLOYEE_DELETE_TITLE)" @close="emit('close')">
    <p v-if="employee">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE_QUESTION, formatEmployeeFullName(employee)) }}
    </p>
    <p v-if="isSelf" class="alert alert--error">
      {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE_SELF_WARNING) }}
    </p>

    <p v-if="error" class="field__error">{{ error }}</p>

    <div class="form__actions">
      <button type="button" class="btn btn--danger" @click="emit('confirm')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_DELETE) }}
      </button>
      <button type="button" class="btn btn--ghost" @click="emit('close')">
        {{ tr(RESOURCE_KEYS.EMPLOYEE_CANCEL) }}
      </button>
    </div>
  </ModalDialog>
</template>
