// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const craneTableContainer = document.getElementById('craneTableContainer');
const craneTableBody = document.getElementById('craneTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Modal Elements
const addCraneModal = new bootstrap.Modal(document.getElementById('addCraneModal'));
const editCraneModal = new bootstrap.Modal(document.getElementById('editCraneModal'));
const deleteCraneModal = new bootstrap.Modal(document.getElementById('deleteCraneModal'));
const maintenanceLogModal = new bootstrap.Modal(document.getElementById('maintenanceLogModal'));

// Form Elements
const addCraneForm = document.getElementById('addCraneForm');
const editCraneForm = document.getElementById('editCraneForm');
const addCraneStatus = document.getElementById('addCraneStatus');
const editCraneStatus = document.getElementById('editCraneStatus');
const addMaintenanceFields = document.getElementById('addMaintenanceFields');
const editMaintenanceFields = document.getElementById('editMaintenanceFields');

// Buttons
const saveNewCraneBtn = document.getElementById('saveNewCraneBtn');
const updateCraneBtn = document.getElementById('updateCraneBtn');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

// Toast
const notificationToast = document.getElementById('notificationToast');
const toast = new bootstrap.Toast(notificationToast);

// Global variables
let cranes = [];
let currentCraneId = null;

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
  loadCranes();
  setupEventListeners();
});

// Function to load cranes
async function loadCranes() {
  showLoading();

  try {
    const response = await fetch('/api/Cranes');

    if (!response.ok) {
      throw new Error('Failed to fetch cranes');
    }

    cranes = await response.json();
    console.log('Loaded cranes:', cranes);

    if (cranes.length === 0) {
      showNoData();
      return;
    }

    renderCraneTable();
    showTable();
  } catch (error) {
    console.error('Error loading cranes:', error);
    showError();
  }
}

// Check if status is "Available" or 0
function isAvailableStatus(status) {
  return status === 0 || status === "Available";
}

// Render the crane table
function renderCraneTable() {
  craneTableBody.innerHTML = '';

  cranes.forEach(crane => {
    const row = document.createElement('tr');

    // Format the status badge
    const isAvailable = isAvailableStatus(crane.status);
    const statusClass = isAvailable ? 'status-available' : 'status-maintenance';
    const statusText = isAvailable ? 'Available' : 'Maintenance';

    row.innerHTML = `
      <td>${crane.code}</td>
      <td>${crane.capacity}</td>
      <td><span class="status-badge ${statusClass}">${statusText}</span></td>
      <td id="maintenance-info-${crane.id}">
        ${!isAvailable ? '<span class="text-muted">Loading...</span>' : '-'}
      </td>
      <td>
        <div class="action-buttons">
          <button type="button" class="btn btn-primary edit-crane-btn" data-id="${crane.id}">
            <i class="bx bx-edit"></i> Edit
          </button>
          <button type="button" class="btn btn-danger delete-crane-btn" data-id="${crane.id}" data-code="${crane.code}">
            <i class="bx bx-trash"></i> Delete
          </button>
          <button type="button" class="btn btn-info maintenance-log-btn" data-id="${crane.id}" data-code="${crane.code}">
            <i class="bx bx-history"></i> Logs
          </button>
        </div>
      </td>
    `;

    craneTableBody.appendChild(row);

    // If crane is in maintenance, fetch maintenance info
    if (!isAvailable) {
      fetchMaintenanceInfo(crane.id);
    }
  });

  // Add event listeners to the action buttons
  document.querySelectorAll('.edit-crane-btn').forEach(btn => {
    btn.addEventListener('click', function() {
      const craneId = parseInt(this.dataset.id);
      openEditCraneModal(craneId);
    });
  });

  document.querySelectorAll('.delete-crane-btn').forEach(btn => {
    btn.addEventListener('click', function() {
      const craneId = parseInt(this.dataset.id);
      const craneCode = this.dataset.code;
      openDeleteModal(craneId, craneCode);
    });
  });

  document.querySelectorAll('.maintenance-log-btn').forEach(btn => {
    btn.addEventListener('click', function() {
      const craneId = parseInt(this.dataset.id);
      const craneCode = this.dataset.code;
      openMaintenanceLogModal(craneId, craneCode);
    });
  });
}

// Fetch maintenance info for a crane
async function fetchMaintenanceInfo(craneId) {
  try {
    const response = await fetch(`/api/Cranes/${craneId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch crane details');
    }

    const craneDetail = await response.json();
    const maintenanceInfoCell = document.getElementById(`maintenance-info-${craneId}`);

    if (!maintenanceInfoCell) return;

    // If there are urgent logs
    if (craneDetail.urgentLogs && craneDetail.urgentLogs.length > 0) {
      const latestLog = craneDetail.urgentLogs[0]; // Assuming logs are ordered by date

      // Format dates
      const startDate = new Date(latestLog.urgentStartTime);
      const endDate = new Date(latestLog.urgentEndTime);
      const startFormatted = formatDate(startDate);
      const endFormatted = formatDate(endDate);

      // Calculate remaining time
      const now = new Date();
      let remainingText = '';

      if (endDate > now) {
        const remainingMs = endDate - now;
        const remainingDays = Math.floor(remainingMs / (1000 * 60 * 60 * 24));
        const remainingHours = Math.floor((remainingMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

        remainingText = `<div class="maintenance-timer">Remaining: ${remainingDays}d ${remainingHours}h</div>`;
      } else {
        remainingText = `<div class="maintenance-timer text-success">Maintenance period ended</div>`;
      }

      maintenanceInfoCell.innerHTML = `
        <div>${startFormatted} - ${endFormatted}</div>
        ${remainingText}
        <div class="text-truncate" title="${latestLog.reasons}" style="max-width: 200px;">
          ${latestLog.reasons}
        </div>
      `;
    } else {
      maintenanceInfoCell.innerHTML = `<span class="text-muted">No maintenance logs</span>`;
    }
  } catch (error) {
    console.error('Error fetching maintenance info:', error);
    const maintenanceInfoCell = document.getElementById(`maintenance-info-${craneId}`);
    if (maintenanceInfoCell) {
      maintenanceInfoCell.innerHTML = `<span class="text-danger">Error loading info</span>`;
    }
  }
}

// Setup event listeners
function setupEventListeners() {
  // Add crane form status change
  addCraneStatus.addEventListener('change', function() {
    addMaintenanceFields.style.display = this.value === '1' ? 'block' : 'none';
  });

  // Edit crane form status change
  editCraneStatus.addEventListener('change', function() {
    editMaintenanceFields.style.display = this.value === '1' ? 'block' : 'none';
  });

  // Save new crane button
  saveNewCraneBtn.addEventListener('click', saveNewCrane);

  // Update crane button
  updateCraneBtn.addEventListener('click', updateCrane);

  // Confirm delete button
  confirmDeleteBtn.addEventListener('click', deleteCrane);

  // Reset form when modals are closed
  document.getElementById('addCraneModal').addEventListener('hidden.bs.modal', function() {
    addCraneForm.reset();
    addMaintenanceFields.style.display = 'none';
    clearValidationErrors(addCraneForm);
  });

  document.getElementById('editCraneModal').addEventListener('hidden.bs.modal', function() {
    editCraneForm.reset();
    editMaintenanceFields.style.display = 'none';
    clearValidationErrors(editCraneForm);
  });
}

// Clear validation errors from a form
function clearValidationErrors(form) {
  form.querySelectorAll('.is-invalid').forEach(input => {
    input.classList.remove('is-invalid');
  });
}

// Open the edit crane modal
function openEditCraneModal(craneId) {
  const crane = cranes.find(c => c.id === craneId);

  if (!crane) {
    showNotification('Error finding crane data', 'danger');
    return;
  }

  currentCraneId = craneId;

  // Populate form fields
  document.getElementById('editCraneId').value = crane.id;
  document.getElementById('editCraneCode').value = crane.code;
  document.getElementById('editCraneCapacity').value = crane.capacity;

  // Handle status value (string or numeric)
  const statusValue = isAvailableStatus(crane.status) ? 0 : 1;
  document.getElementById('editCraneStatus').value = statusValue;

  // Show/hide maintenance fields based on status
  editMaintenanceFields.style.display = statusValue === 1 ? 'block' : 'none';

  // Show the modal
  editCraneModal.show();
}

// Open the delete confirmation modal
function openDeleteModal(craneId, craneCode) {
  currentCraneId = craneId;
  document.getElementById('deleteCraneCode').textContent = craneCode;
  deleteCraneModal.show();
}

// Open the maintenance log modal
async function openMaintenanceLogModal(craneId, craneCode) {
  document.getElementById('maintenanceLogCraneTitle').textContent = `Maintenance History for ${craneCode}`;
  document.getElementById('maintenanceLogLoading').style.display = 'block';
  document.getElementById('maintenanceLogTable').style.display = 'none';
  document.getElementById('noMaintenanceLogMessage').style.display = 'none';

  maintenanceLogModal.show();

  try {
    const response = await fetch(`/api/Cranes/${craneId}/UrgentLogs`);

    if (!response.ok) {
      throw new Error('Failed to fetch maintenance logs');
    }

    const logs = await response.json();

    document.getElementById('maintenanceLogLoading').style.display = 'none';

    if (logs.length === 0) {
      document.getElementById('noMaintenanceLogMessage').style.display = 'block';
      return;
    }

    const tableBody = document.getElementById('maintenanceLogTableBody');
    tableBody.innerHTML = '';

    logs.forEach(log => {
      const startDate = new Date(log.urgentStartTime);
      const endDate = new Date(log.urgentEndTime);

      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${formatDateTime(startDate)}</td>
        <td>${formatDateTime(endDate)}</td>
        <td>${log.estimatedUrgentDays} days, ${log.estimatedUrgentHours} hours</td>
        <td>${log.reasons}</td>
      `;

      tableBody.appendChild(row);
    });

    document.getElementById('maintenanceLogTable').style.display = 'table';
  } catch (error) {
    console.error('Error fetching maintenance logs:', error);
    document.getElementById('maintenanceLogLoading').style.display = 'none';
    document.getElementById('noMaintenanceLogMessage').style.display = 'block';
    document.getElementById('noMaintenanceLogMessage').innerHTML = `
      <p class="text-danger mb-0">Error loading maintenance logs. Please try again.</p>
    `;
  }
}

// Save a new crane
async function saveNewCrane() {
  // Get form values
  const code = document.getElementById('addCraneCode').value.trim();
  const capacity = parseInt(document.getElementById('addCraneCapacity').value);
  const status = parseInt(document.getElementById('addCraneStatus').value);

  // Clear previous validation errors
  clearValidationErrors(addCraneForm);

  // Validate form
  if (!code) {
    document.getElementById('addCraneCode').classList.add('is-invalid');
    return;
  }

  if (isNaN(capacity) || capacity <= 0) {
    document.getElementById('addCraneCapacity').classList.add('is-invalid');
    return;
  }

  // Create crane object with status as string
  const craneData = {
    code: code,
    capacity: capacity,
    status: status === 1 ? "Maintenance" : "Available"  // Status as string
  };

  let payload;

  // If maintenance status is selected, validate maintenance fields
  if (status === 1) {
    const days = parseInt(document.getElementById('addMaintenanceDays').value) || 0;
    const hours = parseInt(document.getElementById('addMaintenanceHours').value) || 0;
    const reasons = document.getElementById('addMaintenanceReasons').value.trim();

    if (days === 0 && hours === 0) {
      showNotification('Please specify maintenance duration', 'danger');
      return;
    }

    if (!reasons) {
      document.getElementById('addMaintenanceReasons').classList.add('is-invalid');
      return;
    }

    // Add UrgentLog data
    const urgentLogData = {
      urgentStartTime: new Date().toISOString(),  // Format ISO string
      estimatedUrgentDays: days,
      estimatedUrgentHours: hours,
      reasons: reasons
    };

    // Create request payload with crane and urgentLog
    payload = {
      crane: craneData,
      urgentLog: urgentLogData
    };
  } else {
    // If no maintenance, just send the crane data
    payload = craneData;  // Use crane data directly
  }

  // Log for debugging
  console.log('Saving new crane, payload:', JSON.stringify(status === 1 ? payload : craneData));

  try {
    // Send request to API
    const response = await fetch('/api/Cranes', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(status === 1 ? payload : craneData)
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Error response:', errorText);
      throw new Error('Failed to create crane');
    }

    addCraneModal.hide();
    showNotification('Crane added successfully', 'success');
    loadCranes();
  } catch (error) {
    console.error('Error creating crane:', error);
    showNotification(error.message || 'Error creating crane', 'danger');
  }
}

// Update an existing crane
async function updateCrane() {
  // Get form values
  const craneId = parseInt(document.getElementById('editCraneId').value);
  const code = document.getElementById('editCraneCode').value.trim();
  const capacity = parseInt(document.getElementById('editCraneCapacity').value);
  const status = parseInt(document.getElementById('editCraneStatus').value);

  // Clear previous validation errors
  clearValidationErrors(editCraneForm);

  // Validate form
  if (!code) {
    document.getElementById('editCraneCode').classList.add('is-invalid');
    return;
  }

  if (isNaN(capacity) || capacity <= 0) {
    document.getElementById('editCraneCapacity').classList.add('is-invalid');
    return;
  }

  // Create crane object with status as string
  const craneData = {
    code: code,
    capacity: capacity,
    status: status === 1 ? "Maintenance" : "Available"  // Status as string
  };

  // Create payload structure
  let payload = {
    crane: craneData
  };

  // If maintenance status is selected, validate maintenance fields
  if (status === 1) {
    const days = parseInt(document.getElementById('editMaintenanceDays').value) || 0;
    const hours = parseInt(document.getElementById('editMaintenanceHours').value) || 0;
    const reasons = document.getElementById('editMaintenanceReasons').value.trim();

    // Check if the status has changed from Available to Maintenance
    const originalCrane = cranes.find(c => c.id === craneId);
    const statusChanged = originalCrane && isAvailableStatus(originalCrane.status);

    if (statusChanged) {
      if (days === 0 && hours === 0) {
        showNotification('Please specify maintenance duration', 'danger');
        return;
      }

      if (!reasons) {
        document.getElementById('editMaintenanceReasons').classList.add('is-invalid');
        return;
      }

      // Add UrgentLog data
      payload.urgentLog = {
        urgentStartTime: new Date().toISOString(),  // Format ISO string
        estimatedUrgentDays: days,
        estimatedUrgentHours: hours,
        reasons: reasons
      };
    }
  }

  // Log for debugging
  console.log('Updating crane, payload:', JSON.stringify(payload));

  try {
    // Send request to API
    const response = await fetch(`/api/Cranes/${craneId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Error response:', errorText);
      throw new Error('Failed to update crane');
    }

    editCraneModal.hide();
    showNotification('Crane updated successfully', 'success');
    loadCranes();
  } catch (error) {
    console.error('Error updating crane:', error);
    showNotification('Error updating crane', 'danger');
  }
}

// Delete a crane
async function deleteCrane() {
  if (!currentCraneId) return;

  try {
    const response = await fetch(`/api/Cranes/${currentCraneId}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Error response:', errorText);
      throw new Error('Failed to delete crane');
    }

    deleteCraneModal.hide();
    showNotification('Crane deleted successfully', 'success');
    loadCranes();
  } catch (error) {
    console.error('Error deleting crane:', error);
    showNotification('Error deleting crane. It may be referenced by existing bookings.', 'danger');
  }
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

// Format date as DD-MM-YYYY
function formatDate(date) {
  return date.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).replace(/\//g, '-');
}

// Format date and time as DD-MM-YYYY HH:MM
function formatDateTime(date) {
  return date.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).replace(/\//g, '-');
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  craneTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  craneTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  craneTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  craneTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
