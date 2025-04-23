// wwwroot/js/crane-management/breakdown-history.js
// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const breakdownsContainer = document.getElementById('breakdownsContainer');
const breakdownTableBody = document.getElementById('breakdownTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Filter elements
const craneFilter = document.getElementById('craneFilter');
const statusFilter = document.getElementById('statusFilter');
const dateFromFilter = document.getElementById('dateFromFilter');
const dateToFilter = document.getElementById('dateToFilter');
const applyFiltersBtn = document.getElementById('applyFiltersBtn');
const clearFiltersBtn = document.getElementById('clearFiltersBtn');
const refreshBtn = document.getElementById('refreshBtn');

// Modals
const breakdownDetailModal = new bootstrap.Modal(document.getElementById('breakdownDetailModal'));

// Toast
const notificationToast = document.getElementById('notificationToast');
const toast = new bootstrap.Toast(notificationToast);

// Global variables
let breakdowns = [];
let cranes = [];
let filters = {
  craneId: '',
  status: '',
  dateFrom: '',
  dateTo: ''
};

// Check if there's a pre-selected craneId from the URL
const urlParams = new URLSearchParams(window.location.search);
const preSelectedCraneId = urlParams.get('craneId');

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
  // If the craneId is passed from the URL, set it in the filters
  if (preSelectedCraneId) {
    filters.craneId = preSelectedCraneId;
  }

  loadCranes().then(() => {
    loadBreakdowns();
  });

  setupEventListeners();
});

// Function to load all cranes for filter dropdown
async function loadCranes() {
  try {
    const response = await fetch('/api/Cranes');

    if (!response.ok) {
      throw new Error('Failed to fetch cranes');
    }

    cranes = await response.json();
    populateCraneFilter();

    // If there's a pre-selected craneId, set the dropdown
    if (preSelectedCraneId) {
      craneFilter.value = preSelectedCraneId;
    }
  } catch (error) {
    console.error('Error loading cranes:', error);
    showNotification('Failed to load crane filter data', 'danger');
  }
}

// Populate crane filter dropdown
function populateCraneFilter() {
  craneFilter.innerHTML = '<option value="">All Cranes</option>';

  cranes.forEach(crane => {
    const option = document.createElement('option');
    option.value = crane.id;
    option.textContent = crane.code;

    // If this is the pre-selected crane, mark it as selected
    if (crane.id.toString() === preSelectedCraneId) {
      option.selected = true;
    }

    craneFilter.appendChild(option);
  });
}

// Function to load breakdowns with filters
async function loadBreakdowns() {
  showLoading();

  try {
    let url = '/api/Breakdowns';

    // If a specific crane is selected, use the specific API endpoint
    if (filters.craneId) {
      url = `/api/Breakdowns/crane/${filters.craneId}`;
    }

    const response = await fetch(url);

    if (!response.ok) {
      throw new Error('Failed to fetch breakdown records');
    }

    breakdowns = await response.json();
    console.log('Loaded breakdowns:', breakdowns);

    // Apply client-side filters
    const filteredBreakdowns = filterBreakdowns(breakdowns);

    if (filteredBreakdowns.length === 0) {
      showNoData();
      return;
    }

    renderBreakdownTable(filteredBreakdowns);
    showBreakdowns();
  } catch (error) {
    console.error('Error loading breakdowns:', error);
    showError();
  }
}

// Apply filters on the client side
function filterBreakdowns(data) {
  return data.filter(breakdown => {
    // Skip if the data was already filtered by craneId on the server
    const craneMatches = !filters.craneId || breakdown.craneId.toString() === filters.craneId;

    // Filter by status
    let statusMatches = true;
    if (filters.status) {
      const now = new Date();
      const endDate = new Date(breakdown.urgentEndTime);
      const isCompleted = breakdown.actualUrgentEndTime !== null;
      const isOverdue = !isCompleted && endDate < now;

      switch (filters.status) {
        case 'completed':
          statusMatches = isCompleted;
          break;
        case 'ongoing':
          statusMatches = !isCompleted && !isOverdue;
          break;
        case 'overdue':
          statusMatches = isOverdue;
          break;
      }
    }

    // Filter by date range
    let dateMatches = true;
    const startDate = new Date(breakdown.urgentStartTime);

    if (filters.dateFrom) {
      const fromDate = new Date(filters.dateFrom);
      fromDate.setHours(0, 0, 0, 0);
      dateMatches = dateMatches && startDate >= fromDate;
    }

    if (filters.dateTo) {
      const toDate = new Date(filters.dateTo);
      toDate.setHours(23, 59, 59, 999);
      dateMatches = dateMatches && startDate <= toDate;
    }

    return craneMatches && statusMatches && dateMatches;
  });
}

// Render the breakdown table
function renderBreakdownTable(data) {
  breakdownTableBody.innerHTML = '';

  data.forEach(breakdown => {
    const now = new Date();
    const endDate = new Date(breakdown.urgentEndTime);
    const isCompleted = breakdown.actualUrgentEndTime !== null;
    const isOverdue = !isCompleted && endDate < now;

    let statusClass = '';
    let statusText = '';

    if (isCompleted) {
      statusClass = 'status-completed';
      statusText = 'Completed';
    } else if (isOverdue) {
      statusClass = 'status-overdue';
      statusText = 'Overdue';
    } else {
      statusClass = 'status-ongoing';
      statusText = 'Ongoing';
    }

    const row = document.createElement('tr');
    row.className = 'breakdown-row';
    row.dataset.id = breakdown.id;

    // Format dates
    const startDate = new Date(breakdown.urgentStartTime);
    const estimatedEndDate = new Date(breakdown.urgentEndTime);
    const actualEndDate = breakdown.actualUrgentEndTime ? new Date(breakdown.actualUrgentEndTime) : null;

    const startFormatted = formatDateTime(startDate);
    const endFormatted = formatDateTime(estimatedEndDate);
    const actualEndFormatted = actualEndDate ? formatDateTime(actualEndDate) : 'N/A';

    // Create duration text
    const durationText = `${breakdown.estimatedUrgentDays} days, ${breakdown.estimatedUrgentHours} hours`;

    // Create row content
    row.innerHTML = `
            <td>${breakdown.craneCode || getCraneCode(breakdown.craneId) || 'Unknown'}</td>
            <td>${startFormatted}</td>
            <td>${endFormatted}</td>
            <td>${durationText}</td>
            <td>${actualEndFormatted}</td>
            <td><span class="status-badge ${statusClass}">${statusText}</span></td>
            <td>
                <span class="d-inline-block text-truncate" style="max-width: 200px;">${breakdown.reasons}</span>
                <button type="button" class="btn btn-sm btn-link p-0 ms-2 view-details-btn">
                    <i class="bx bx-info-circle"></i>
                </button>
            </td>
        `;

    breakdownTableBody.appendChild(row);
  });

  // Add event listeners to view details buttons
  document.querySelectorAll('.view-details-btn').forEach(btn => {
    btn.addEventListener('click', function () {
      const row = this.closest('.breakdown-row');
      const breakdownId = parseInt(row.dataset.id);
      openBreakdownDetailModal(breakdownId);
    });
  });
}

// Get crane code by ID
function getCraneCode(craneId) {
  const crane = cranes.find(c => c.id === craneId);
  return crane ? crane.code : null;
}

// Open breakdown detail modal
function openBreakdownDetailModal(breakdownId) {
  let breakdown;

  // Find the breakdown in the data
  if (filters.craneId) {
    // If viewing a specific crane's breakdowns
    breakdown = breakdowns.find(b => b.id === breakdownId);
  } else {
    // If viewing all breakdowns
    breakdown = breakdowns.find(b => b.id === breakdownId);
  }

  if (!breakdown) {
    showNotification('Error finding breakdown data', 'danger');
    return;
  }

  // Get crane details
  const craneCode = breakdown.craneCode || getCraneCode(breakdown.craneId) || 'Unknown';
  const craneCapacity = breakdown.craneCapacity || getCraneCapacity(breakdown.craneId) || 'N/A';

  // Format dates
  const startDate = new Date(breakdown.urgentStartTime);
  const estimatedEndDate = new Date(breakdown.urgentEndTime);
  const actualEndDate = breakdown.actualUrgentEndTime ? new Date(breakdown.actualUrgentEndTime) : null;

  const startFormatted = formatDateTime(startDate);
  const endFormatted = formatDateTime(estimatedEndDate);
  const actualEndFormatted = actualEndDate ? formatDateTime(actualEndDate) : 'N/A';

  // Calculate durations
  const estimatedDuration = `${breakdown.estimatedUrgentDays} days, ${breakdown.estimatedUrgentHours} hours`;

  let actualDuration = 'N/A';
  if (actualEndDate) {
    const durationMs = actualEndDate - startDate;
    const durationDays = Math.floor(durationMs / (1000 * 60 * 60 * 24));
    const durationHours = Math.floor((durationMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    actualDuration = `${durationDays} days, ${durationHours} hours`;
  }

  // Determine status
  const now = new Date();
  const isCompleted = actualEndDate !== null;
  const isOverdue = !isCompleted && estimatedEndDate < now;

  let statusClass = '';
  let statusText = '';

  if (isCompleted) {
    statusClass = 'status-completed';
    statusText = 'Completed';
  } else if (isOverdue) {
    statusClass = 'status-overdue';
    statusText = 'Overdue';
  } else {
    statusClass = 'status-ongoing';
    statusText = 'Ongoing';
  }

  // Populate modal
  document.getElementById('detailCraneCode').textContent = craneCode;
  document.getElementById('detailCraneCapacity').textContent = craneCapacity;
  document.getElementById('detailStartDate').textContent = startFormatted;
  document.getElementById('detailEndDate').textContent = endFormatted;
  document.getElementById('detailActualEndDate').textContent = actualEndFormatted;
  document.getElementById('detailEstimatedDuration').textContent = estimatedDuration;
  document.getElementById('detailActualDuration').textContent = actualDuration;

  const statusElement = document.getElementById('detailStatus');
  statusElement.className = `status-badge ${statusClass}`;
  statusElement.textContent = statusText;

  document.getElementById('detailReasons').textContent = breakdown.reasons || 'No reason provided';

  // Show the modal
  breakdownDetailModal.show();
}

// Get crane capacity by ID
function getCraneCapacity(craneId) {
  const crane = cranes.find(c => c.id === craneId);
  return crane ? crane.capacity : null;
}

// Setup event listeners
function setupEventListeners() {
  // Apply filters button
  applyFiltersBtn.addEventListener('click', function () {
    // Update filters object
    filters.craneId = craneFilter.value;
    filters.status = statusFilter.value;
    filters.dateFrom = dateFromFilter.value;
    filters.dateTo = dateToFilter.value;

    // Reload data with new filters
    loadBreakdowns();
  });

  // Clear filters button
  clearFiltersBtn.addEventListener('click', function () {
    // Reset form fields
    craneFilter.value = '';
    statusFilter.value = '';
    dateFromFilter.value = '';
    dateToFilter.value = '';

    // Clear filters object
    filters = {
      craneId: '',
      status: '',
      dateFrom: '',
      dateTo: ''
    };

    // Reload data without filters
    loadBreakdowns();
  });

  // Refresh button
  refreshBtn.addEventListener('click', function () {
    loadBreakdowns();
  });
}

// Show a notification toast
function showNotification(message, type = 'success') {
  const toastElement = document.getElementById('notificationToast');
  const toastBody = toastElement.querySelector('.toast-body');

  // Set the message
  toastBody.textContent = message;

  // Set the toast color based on type
  toastElement.className = `toast align-items-center text-white bg-${type} border-0`;

  // Show the toast
  const toastInstance = bootstrap.Toast.getOrCreateInstance(toastElement);
  toastInstance.show();
}

// Format date and time as DD-MM-YYYY HH:MM
function formatDateTime(date) {
  return date
    .toLocaleDateString('en-GB', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
    .replace(/\//g, '-');
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  breakdownsContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showBreakdowns() {
  loadingIndicator.style.display = 'none';
  breakdownsContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  breakdownsContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  breakdownsContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
