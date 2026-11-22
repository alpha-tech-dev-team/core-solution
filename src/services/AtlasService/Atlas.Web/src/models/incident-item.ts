export interface IncidentItem {
  id: string
  ticketNumber: number
  employeeFullName: string
  categoryName: string
  roomName: string | null
  hardwareName: string | null
  softwareName: string | null
  userDescription: string
  status: string
  priority: string
  assignedEngineerFullName: string | null
  createdAt: string
}
