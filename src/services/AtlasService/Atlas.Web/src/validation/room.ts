import { z } from 'zod'
import { ROOM_LIMITS, VALIDATION_RULES } from '@/constants/validation'

export const roomFormSchema = z.object({
  name: z
    .string()
    .min(1, VALIDATION_RULES.REQUIRED)
    .max(ROOM_LIMITS.NAME_MAX, VALIDATION_RULES.ROOM_NAME_TOO_LONG),
  description: z
    .string()
    .max(ROOM_LIMITS.DESCRIPTION_MAX, VALIDATION_RULES.ROOM_DESCRIPTION_TOO_LONG),
})

export type RoomForm = z.infer<typeof roomFormSchema>
