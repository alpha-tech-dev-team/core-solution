import { firstValidationMessage } from '@shared/validation/validation-message'
import type { ValidationMessage } from '@shared/validation/validation-message'
import { roomFormSchema } from '@/validation/room'

export interface RoomFormValues {
  name: string
  description: string
}

export function validateRoomForm(values: RoomFormValues): ValidationMessage | null {
  const result = roomFormSchema.safeParse(values)

  return result.success ? null : firstValidationMessage(result.error)
}
