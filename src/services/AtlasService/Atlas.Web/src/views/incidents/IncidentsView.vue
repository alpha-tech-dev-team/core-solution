<script setup lang="ts">
import LogoutButton from '@shared/components/LogoutButton.vue'
import { LOADING_PLACEHOLDER } from '@shared/constants/ui'
import { useTranslations } from '@shared/i18n/create-i18n'
import { RESOURCE_KEYS } from '@/constants/resource-keys'
import { ROUTE_NAMES } from '@/constants/routes'
import PaginationNav from '@shared/components/PaginationNav.vue'
import { useIncidentsView } from './use-incidents-view'

const {
  incidents,
  page: currentPage,
  totalPages,
  isLoading,
  errorMessage,
  changePage,
} = useIncidentsView()
const { tr, trStatus } = useTranslations()
</script>

<template>
  <div class="page">
    <header class="page__header">
      <h1>{{ tr(RESOURCE_KEYS.APP_TITLE) }}</h1>
      <nav class="page__nav">
        <RouterLink class="btn btn--ghost btn--sm" :to="{ name: ROUTE_NAMES.HOME }">
          {{ tr(RESOURCE_KEYS.NAV_HOME) }}
        </RouterLink>
        <LogoutButton />
      </nav>
    </header>

    <main class="page__body">
      <div class="toolbar">
        <h2>{{ tr(RESOURCE_KEYS.INCIDENTS_TITLE) }}</h2>
      </div>

      <p v-if="errorMessage" class="alert alert--error">{{ errorMessage }}</p>
      <p v-if="isLoading" class="text-muted">{{ LOADING_PLACEHOLDER }}</p>

      <p v-else-if="incidents.length === 0" class="text-muted">
        {{ tr(RESOURCE_KEYS.INCIDENTS_EMPTY) }}
      </p>

      <table v-else class="table">
        <thead>
          <tr>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_NUMBER) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_CREATED_AT) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_EMPLOYEE) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_CATEGORY) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_ROOM) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_ASSET) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_DESCRIPTION) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_STATUS) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_PRIORITY) }}</th>
            <th>{{ tr(RESOURCE_KEYS.INCIDENT_ASSIGNED_ENGINEER) }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="incident in incidents" :key="incident.id">
            <td class="numeric">{{ incident.ticketNumber }}</td>
            <td class="numeric">{{ incident.createdAt }}</td>
            <td>{{ incident.employeeFullName }}</td>
            <td>{{ incident.categoryName }}</td>
            <td>{{ incident.roomName ?? tr(RESOURCE_KEYS.INCIDENT_NO_ROOM) }}</td>
            <td>{{ incident.hardwareName ?? incident.softwareName ?? tr(RESOURCE_KEYS.INCIDENT_NO_ASSET) }}</td>
            <td>{{ incident.userDescription }}</td>
            <td>{{ trStatus(incident.status) }}</td>
            <td>{{ trStatus(incident.priority) }}</td>
            <td>{{ incident.assignedEngineerFullName ?? tr(RESOURCE_KEYS.INCIDENT_NO_ASSIGNEE) }}</td>
          </tr>
        </tbody>
      </table>

      <PaginationNav
        :page="currentPage"
        :total-pages="totalPages"
        :prev-label="tr(RESOURCE_KEYS.INCIDENTS_PAGE_PREV)"
        :next-label="tr(RESOURCE_KEYS.INCIDENTS_PAGE_NEXT)"
        :info-label="tr(RESOURCE_KEYS.INCIDENTS_PAGE_INFO, currentPage, totalPages)"
        @change="changePage($event)"
      />
    </main>
  </div>
</template>
