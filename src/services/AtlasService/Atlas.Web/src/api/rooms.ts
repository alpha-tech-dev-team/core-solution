import { api } from '@shared/app/app-session'
import { PAGINATION_QUERY, ROOM_ENDPOINTS } from '@/constants/api'
import type { CreateRoomPayload } from '@/models/create-room-payload'
import type { RoomItem } from '@/models/room-item'
import type { UpdateRoomPayload } from '@/models/update-room-payload'
import type { Page } from '@shared/models/page'

export async function fetchRooms(page: number, pageSize: number): Promise<Page<RoomItem>> {
  return await api.get<Page<RoomItem>>(ROOM_ENDPOINTS.COLLECTION, {
    [PAGINATION_QUERY.PAGE]: page,
    [PAGINATION_QUERY.PAGE_SIZE]: pageSize,
  })
}

export async function createRoom(payload: CreateRoomPayload): Promise<RoomItem> {
  return await api.post<RoomItem>(ROOM_ENDPOINTS.COLLECTION, payload)
}

export async function updateRoom(roomId: string, payload: UpdateRoomPayload): Promise<RoomItem> {
  return await api.patch<RoomItem>(ROOM_ENDPOINTS.BY_ID(roomId), payload)
}

export async function deleteRoom(roomId: string): Promise<void> {
  await api.delete(ROOM_ENDPOINTS.BY_ID(roomId))
}
