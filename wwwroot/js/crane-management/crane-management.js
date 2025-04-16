// // DOM Elements
// const loadingIndicator = document.getElementById('loadingIndicator');
// const craneTableContainer = document.getElementById('craneTableContainer');
// const craneTableBody = document.getElementById('craneTableBody');
// const noDataMessage = document.getElementById('noDataMessage');
// const errorMessage = document.getElementById('errorMessage');

// // Modal Elements
// const addCraneModal = new bootstrap.Modal(document.getElementById('addCraneModal'));
// const editCraneModal = new bootstrap.Modal(document.getElementById('editCraneModal'));
// const deleteCraneModal = new bootstrap.Modal(document.getElementById('deleteCraneModal'));
// const maintenanceLogModal = new bootstrap.Modal(document.getElementById('maintenanceLogModal'));

// // Form Elements
// const addCraneForm = document.getElementById('addCraneForm');
// const editCraneForm = document.getElementById('editCraneForm');
// const addCraneStatus = document.getElementById('addCraneStatus');
// const editCraneStatus = document.getElementById('editCraneStatus');
// const addMaintenanceFields = document.getElementById('addMaintenanceFields');
// const editMaintenanceFields = document.getElementById('editMaintenanceFields');

// // Buttons
// const saveNewCraneBtn = document.getElementById('saveNewCraneBtn');
// const updateCraneBtn = document.getElementById('updateCraneBtn');
// const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

// // Toast
// const notificationToast = document.getElementById('notificationToast');
// const toast = new bootstrap.Toast(notificationToast);

// // Global variables
// let cranes = [];
// let currentCraneId = null;

// // Initialize when the DOM is loaded
// document.addEventListener('DOMContentLoaded', function() {
//   loadCranes();
//   setupEventListeners();
// });

// // Function to load cranes
// async function loadCranes() {
//   showLoading();

//   try {
//     const response = await fetch('/api/Cranes');

//     if (!response.ok) {
//       throw new Error('Failed to fetch cranes');
//     }

//     cranes = await response.json();
//     console.log('Loaded cranes:', cranes);

//     if (cranes.length === 0) {
//       showNoData();
//       return;
//     }

//     renderCraneTable();
//     showTable();
//   } catch (error) {
//     console.error('Error loading cranes:', error);
//     showError();
//   }
// }

// // Check if status is "Available" or 0
// function isAvailableStatus(status) {
//   return status === 0 || status === "Available";
// }

// // Render the crane table
// function renderCraneTable() {
//   craneTableBody.innerHTML = '';

//   cranes.forEach(crane => {
//     const row = document.createElement('tr');

//     // Format the status badge
//     const isAvailable = isAvailableStatus(crane.status);
//     const statusClass = isAvailable ? 'status-available' : 'status-maintenance';
//     const statusText = isAvailable ? 'Available' : 'Maintenance';

//     row.innerHTML = `
//       <td>${crane.code}</td>
//       <td>${crane.capacity}</td>
//       <td><span class="status-badge ${statusClass}">${statusText}</span></td>
//       <td id="maintenance-info-${crane.id}">
//         ${!isAvailable ? '<span class="text-muted">Loading...</span>' : '-'}
//       </td>
//       <td>
//         <div class="action-buttons">
//           <button type="button" class="btn btn-primary edit-crane-btn" data-id="${crane.id}">
//             <i class="bx bx-edit"></i> Edit
//           </button>
//           <button type="button" class="btn btn-danger delete-crane-btn" data-id="${crane.id}" data-code="${crane.code}">
//             <i class="bx bx-trash"></i> Delete
//           </button>
//           <button type="button" class="btn btn-info maintenance-log-btn" data-id="${crane.id}" data-code="${crane.code}">
//             <i class="bx bx-history"></i> Logs
//           </button>
//         </div>
//       </td>
//     `;

//     craneTableBody.appendChild(row);

//     // If crane is in maintenance, fetch maintenance info
//     if (!isAvailable) {
//       fetchMaintenanceInfo(crane.id);
//     }
//   });

//   // Add event listeners to the action buttons
//   document.querySelectorAll('.edit-crane-btn').forEach(btn => {
//     btn.addEventListener('click', function() {
//       const craneId = parseInt(this.dataset.id);
//       openEditCraneModal(craneId);
//     });
//   });

//   document.querySelectorAll('.delete-crane-btn').forEach(btn => {
//     btn.addEventListener('click', function() {
//       const craneId = parseInt(this.dataset.id);
//       const craneCode = this.dataset.code;
//       openDeleteModal(craneId, craneCode);
//     });
//   });

//   document.querySelectorAll('.maintenance-log-btn').forEach(btn => {
//     btn.addEventListener('click', function() {
//       const craneId = parseInt(this.dataset.id);
//       const craneCode = this.dataset.code;
//       openMaintenanceLogModal(craneId, craneCode);
//     });
//   });
// }

// // Fetch maintenance info for a crane
// // async function fetchMaintenanceInfo(craneId) {
// //   try {
// //     const response = await fetch(`/api/Cranes/${craneId}`);

// //     if (!response.ok) {
// //       throw new Error('Failed to fetch crane details');
// //     }

// //     const craneDetail = await response.json();
// //     const maintenanceInfoCell = document.getElementById(`maintenance-info-${craneId}`);

// //     if (!maintenanceInfoCell) return;

// //     // If there are urgent logs
// //     if (craneDetail.urgentLogs && craneDetail.urgentLogs.length > 0) {
// //       const latestLog = craneDetail.urgentLogs[0]; // Assuming logs are ordered by date

// //       // Format dates
// //       const startDate = new Date(latestLog.urgentStartTime);
// //       const estimatedEndDate = new Date(latestLog.urgentEndTime);
// //       const actualEndDate = latestLog.actualUrgentEndTime ? new Date(latestLog.actualUrgentEndTime) : null;

// //       const startFormatted = formatDate(startDate);
// //       const endFormatted = formatDate(estimatedEndDate);

// //       // Calculate remaining time or show actual end time
// //       let timeInfo = '';

// //       if (actualEndDate) {
// //         // Show actual end time if exists
// //         const actualEndFormatted = formatDate(actualEndDate);
// //         timeInfo = `<div class="maintenance-timer text-success">Actual End: ${actualEndFormatted}</div>`;
// //       } else {
// //         // Calculate remaining time
// //         const now = new Date();
// //         if (estimatedEndDate > now) {
// //           const remainingMs = estimatedEndDate - now;
// //           const remainingDays = Math.floor(remainingMs / (1000 * 60 * 60 * 24));
// //           const remainingHours = Math.floor((remainingMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

// //           timeInfo = `<div class="maintenance-timer">Remaining: ${remainingDays}d ${remainingHours}h</div>`;
// //         } else {
// //           timeInfo = `<div class="maintenance-timer text-warning">Estimated time elapsed</div>`;
// //         }
// //       }

// //       maintenanceInfoCell.innerHTML = `
// //         <div>${startFormatted} - ${endFormatted}</div>
// //         ${timeInfo}
// //         <div class="text-truncate" title="${latestLog.reasons}" style="max-width: 200px;">
// //           ${latestLog.reasons}
// //         </div>
// //       `;
// //     } else {
// //       maintenanceInfoCell.innerHTML = `<span class="text-muted">No maintenance logs</span>`;
// //     }
// //   } catch (error) {
// //     console.error('Error fetching maintenance info:', error);
// //     const maintenanceInfoCell = document.getElementById(`maintenance-info-${craneId}`);
// //     if (maintenanceInfoCell) {
// //       maintenanceInfoCell.innerHTML = `<span class="text-danger">Error loading info</span>`;
// //     }
// //   }
// // }
// // Fetch maintenance info for a crane
// async function fetchMaintenanceInfo(craneId) {
//   try {
//     const response = await fetch(`/api/Cranes/${craneId}`);

//     if (!response.ok) {
//       throw new Error('Failed to fetch crane details');
//     }

//     const craneDetail = await response.json();
//     const maintenanceInfoCell = document.getElementById(`maintenance-info-${craneId}`);

//     if (!maintenanceInfoCell) return;

//     // If there are urgent logs
//     if (craneDetail.urgentLogs && craneDetail.urgentLogs.length > 0) {
//       const latestLog = craneDetail.urgentLogs[0]; // Assuming logs are ordered by date

//       // Format dates
//       const startDate = new Date(latestLog.urgentStartTime);
//       const estimatedEndDate = new Date(latestLog.urgentEndTime);
//       const actualEndDate = latestLog.actualUrgentEndTime ? new Date(latestLog.actualUrgentEndTime) : null;

//       const startFormatted = formatDate(startDate);
//       const endFormatted = formatDate(estimatedEndDate);

//       // Calculate remaining time or show actual end time
//       let timeInfo = '';

//       if (actualEndDate) {
//         // Show actual end time if exists
//         const actualEndFormatted = formatDate(actualEndDate);
//         timeInfo = `<div class="maintenance-timer text-success">Actual End: ${actualEndFormatted}</div>`;
//       } else {
//         // Calculate remaining time
//         const now = new Date();
//         if (estimatedEndDate > now) {
//           const remainingMs = estimatedEndDate - now;
//           const remainingDays = Math.floor(remainingMs / (1000 * 60 * 60 * 24));
//           const remainingHours = Math.floor((remainingMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
//           const remainingMinutes = Math.floor((remainingMs % (1000 * 60 * 60)) / (1000 * 60));

//           timeInfo = `<div class="maintenance-timer">Remaining: ${remainingDays}d ${remainingHours}h ${remainingMinutes}m</div>`;
//         } else {
//           timeInfo = `<div class="maintenance-timer text-warning">Estimated time elapsed</div>`;
//         }
//       }

//       maintenanceInfoCell.innerHTML = `
//         <div>${startFormatted} - ${endFormatted}</div>
//         ${timeInfo}
//         <div class="text-truncate" title="${latestLog.reasons}" style="max-width: 200px;">
//           ${latestLog.reasons}
//         </div>
//       `;
//     } else {
//       maintenanceInfoCell.innerHTML = `<span class="text-muted">No maintenance logs</span>`;
//     }
//   } catch (error) {
//     console.error('Error fetching maintenance info:', error);
//     const maintenanceInfoCell = document.getElementById(`maintenance-info-${craneId}`);
//     if (maintenanceInfoCell) {
//       maintenanceInfoCell.innerHTML = `<span class="text-danger">Error loading info</span>`;
//     }
//   }
// }

// // Setup event listeners
// function setupEventListeners() {
//   // Add crane form status change
//   addCraneStatus.addEventListener('change', function() {
//     addMaintenanceFields.style.display = this.value === '1' ? 'block' : 'none';
//   });

//   // Edit crane form status change
//   editCraneStatus.addEventListener('change', function() {
//     editMaintenanceFields.style.display = this.value === '1' ? 'block' : 'none';
//   });

//   // Save new crane button
//   saveNewCraneBtn.addEventListener('click', saveNewCrane);

//   // Update crane button
//   updateCraneBtn.addEventListener('click', updateCrane);

//   // Confirm delete button
//   confirmDeleteBtn.addEventListener('click', deleteCrane);

//   // Reset form when modals are closed
//   document.getElementById('addCraneModal').addEventListener('hidden.bs.modal', function() {
//     addCraneForm.reset();
//     addMaintenanceFields.style.display = 'none';
//     clearValidationErrors(addCraneForm);
//   });

//   document.getElementById('editCraneModal').addEventListener('hidden.bs.modal', function() {
//     editCraneForm.reset();
//     editMaintenanceFields.style.display = 'none';
//     clearValidationErrors(editCraneForm);
//   });
// }

// // Clear validation errors from a form
// function clearValidationErrors(form) {
//   form.querySelectorAll('.is-invalid').forEach(input => {
//     input.classList.remove('is-invalid');
//   });
// }

// // Open the edit crane modal
// function openEditCraneModal(craneId) {
//   const crane = cranes.find(c => c.id === craneId);

//   if (!crane) {
//     showNotification('Error finding crane data', 'danger');
//     return;
//   }

//   currentCraneId = craneId;

//   // Populate form fields
//   document.getElementById('editCraneId').value = crane.id;
//   document.getElementById('editCraneCode').value = crane.code;
//   document.getElementById('editCraneCapacity').value = crane.capacity;

//   // Handle status value (string or numeric)
//   const statusValue = isAvailableStatus(crane.status) ? 0 : 1;
//   document.getElementById('editCraneStatus').value = statusValue;

//   // Show/hide maintenance fields based on status
//   editMaintenanceFields.style.display = statusValue === 1 ? 'block' : 'none';

//   // Show the modal
//   editCraneModal.show();
// }

// // Open the delete confirmation modal
// function openDeleteModal(craneId, craneCode) {
//   currentCraneId = craneId;
//   document.getElementById('deleteCraneCode').textContent = craneCode;
//   deleteCraneModal.show();
// }

// // Open the maintenance log modal
// async function openMaintenanceLogModal(craneId, craneCode) {
//   document.getElementById('maintenanceLogCraneTitle').textContent = `Maintenance History for ${craneCode}`;
//   document.getElementById('maintenanceLogLoading').style.display = 'block';
//   document.getElementById('maintenanceLogTable').style.display = 'none';
//   document.getElementById('noMaintenanceLogMessage').style.display = 'none';

//   maintenanceLogModal.show();

//   try {
//     const response = await fetch(`/api/Cranes/${craneId}/UrgentLogs`);

//     if (!response.ok) {
//       throw new Error('Failed to fetch maintenance logs');
//     }

//     const logs = await response.json();
//     console.log('Maintenance logs received:', logs); // Debug log

//     document.getElementById('maintenanceLogLoading').style.display = 'none';

//     if (logs.length === 0) {
//       document.getElementById('noMaintenanceLogMessage').style.display = 'block';
//       return;
//     }

//     const tableBody = document.getElementById('maintenanceLogTableBody');
//     tableBody.innerHTML = '';

//     logs.forEach(log => {
//       const startDate = new Date(log.urgentStartTime);
//       const endDate = new Date(log.urgentEndTime);
//       const actualEndDate = log.actualUrgentEndTime ? new Date(log.actualUrgentEndTime) : null;

//       const row = document.createElement('tr');

//       // Buat masing-masing sel secara individual untuk memastikan urutan yang benar
//       // Sel 1: Start Date
//       const startDateCell = document.createElement('td');
//       startDateCell.textContent = formatDateTime(startDate);
//       row.appendChild(startDateCell);

//       // Sel 2: Estimated End Date
//       const endDateCell = document.createElement('td');
//       endDateCell.textContent = formatDateTime(endDate);
//       row.appendChild(endDateCell);

//       // Sel 3: Duration
//       const durationCell = document.createElement('td');
//       durationCell.textContent = `${log.estimatedUrgentDays} days, ${log.estimatedUrgentHours} hours`;
//       row.appendChild(durationCell);

//       // Sel 4: Actual End Date
//       const actualEndDateCell = document.createElement('td');
//       actualEndDateCell.textContent = actualEndDate ? formatDateTime(actualEndDate) : 'N/A';
//       row.appendChild(actualEndDateCell);

//       // Sel 5: Reasons
//       const reasonsCell = document.createElement('td');
//       reasonsCell.textContent = log.reasons || 'No reason provided';
//       row.appendChild(reasonsCell);

//       tableBody.appendChild(row);
//     });

//     document.getElementById('maintenanceLogTable').style.display = 'table';
//   } catch (error) {
//     console.error('Error fetching maintenance logs:', error);
//     document.getElementById('maintenanceLogLoading').style.display = 'none';
//     document.getElementById('noMaintenanceLogMessage').style.display = 'block';
//     document.getElementById('noMaintenanceLogMessage').innerHTML = `
//       <p class="text-danger mb-0">Error loading maintenance logs. Please try again.</p>
//     `;
//   }
// }

// // Save a new crane
// async function saveNewCrane() {
//   // Get form values
//   const code = document.getElementById('addCraneCode').value.trim();
//   const capacity = parseInt(document.getElementById('addCraneCapacity').value);
//   const status = parseInt(document.getElementById('addCraneStatus').value);

//   // Clear previous validation errors
//   clearValidationErrors(addCraneForm);

//   // Validate form
//   if (!code) {
//     document.getElementById('addCraneCode').classList.add('is-invalid');
//     return;
//   }

//   if (isNaN(capacity) || capacity <= 0) {
//     document.getElementById('addCraneCapacity').classList.add('is-invalid');
//     return;
//   }

//   // Create crane object with status as string
//   const craneData = {
//     code: code,
//     capacity: capacity,
//     status: status === 1 ? "Maintenance" : "Available"  // Status as string
//   };

//   let payload;

//   // If maintenance status is selected, validate maintenance fields
//   if (status === 1) {
//     const days = parseInt(document.getElementById('addMaintenanceDays').value) || 0;
//     const hours = parseInt(document.getElementById('addMaintenanceHours').value) || 0;
//     const reasons = document.getElementById('addMaintenanceReasons').value.trim();

//     if (days === 0 && hours === 0) {
//       showNotification('Please specify maintenance duration', 'danger');
//       return;
//     }

//     if (!reasons) {
//       document.getElementById('addMaintenanceReasons').classList.add('is-invalid');
//       return;
//     }

//     // Add UrgentLog data
//     const urgentLogData = {
//       urgentStartTime: new Date().toISOString(),  // Format ISO string
//       estimatedUrgentDays: days,
//       estimatedUrgentHours: hours,
//       reasons: reasons
//     };

//     // Create request payload with crane and urgentLog
//     payload = {
//       crane: craneData,
//       urgentLog: urgentLogData
//     };
//   } else {
//     // If no maintenance, just send the crane data
//     payload = craneData;  // Use crane data directly
//   }

//   // Log for debugging
//   console.log('Saving new crane, payload:', JSON.stringify(status === 1 ? payload : craneData));

//   try {
//     // Send request to API
//     const response = await fetch('/api/Cranes', {
//       method: 'POST',
//       headers: {
//         'Content-Type': 'application/json'
//       },
//       body: JSON.stringify(status === 1 ? payload : craneData)
//     });

//     if (!response.ok) {
//       const errorText = await response.text();
//       console.error('Error response:', errorText);
//       throw new Error('Failed to create crane');
//     }

//     addCraneModal.hide();
//     showNotification('Crane added successfully', 'success');
//     loadCranes();
//   } catch (error) {
//     console.error('Error creating crane:', error);
//     showNotification(error.message || 'Error creating crane', 'danger');
//   }
// }

// // Update an existing crane
// async function updateCrane() {
//   // Get form values
//   const craneId = parseInt(document.getElementById('editCraneId').value);
//   const code = document.getElementById('editCraneCode').value.trim();
//   const capacity = parseInt(document.getElementById('editCraneCapacity').value);
//   const status = parseInt(document.getElementById('editCraneStatus').value);

//   // Clear previous validation errors
//   clearValidationErrors(editCraneForm);

//   // Validate form
//   if (!code) {
//     document.getElementById('editCraneCode').classList.add('is-invalid');
//     return;
//   }

//   if (isNaN(capacity) || capacity <= 0) {
//     document.getElementById('editCraneCapacity').classList.add('is-invalid');
//     return;
//   }

//   // Create crane object with status as string
//   const craneData = {
//     code: code,
//     capacity: capacity,
//     status: status === 1 ? "Maintenance" : "Available"  // Status as string
//   };

//   // Create payload structure
//   let payload = {
//     crane: craneData
//   };

//   // If maintenance status is selected, validate maintenance fields
//   if (status === 1) {
//     const days = parseInt(document.getElementById('editMaintenanceDays').value) || 0;
//     const hours = parseInt(document.getElementById('editMaintenanceHours').value) || 0;
//     const reasons = document.getElementById('editMaintenanceReasons').value.trim();

//     // Check if the status has changed from Available to Maintenance
//     const originalCrane = cranes.find(c => c.id === craneId);
//     const statusChanged = originalCrane && isAvailableStatus(originalCrane.status);

//     if (statusChanged) {
//       if (days === 0 && hours === 0) {
//         showNotification('Please specify maintenance duration', 'danger');
//         return;
//       }

//       if (!reasons) {
//         document.getElementById('editMaintenanceReasons').classList.add('is-invalid');
//         return;
//       }

//       // Add UrgentLog data
//       payload.urgentLog = {
//         urgentStartTime: new Date().toISOString(),  // Format ISO string
//         estimatedUrgentDays: days,
//         estimatedUrgentHours: hours,
//         reasons: reasons
//       };
//     }
//   }

//   // Log for debugging
//   console.log('Updating crane, payload:', JSON.stringify(payload));

//   try {
//     // Send request to API
//     const response = await fetch(`/api/Cranes/${craneId}`, {
//       method: 'PUT',
//       headers: {
//         'Content-Type': 'application/json'
//       },
//       body: JSON.stringify(payload)
//     });

//     if (!response.ok) {
//       const errorText = await response.text();
//       console.error('Error response:', errorText);
//       throw new Error('Failed to update crane');
//     }

//     editCraneModal.hide();
//     showNotification('Crane updated successfully', 'success');
//     loadCranes();
//   } catch (error) {
//     console.error('Error updating crane:', error);
//     showNotification('Error updating crane', 'danger');
//   }
// }

// // Delete a crane
// async function deleteCrane() {
//   if (!currentCraneId) return;

//   try {
//     const response = await fetch(`/api/Cranes/${currentCraneId}`, {
//       method: 'DELETE'
//     });

//     if (!response.ok) {
//       const errorText = await response.text();
//       console.error('Error response:', errorText);
//       throw new Error('Failed to delete crane');
//     }

//     deleteCraneModal.hide();
//     showNotification('Crane deleted successfully', 'success');
//     loadCranes();
//   } catch (error) {
//     console.error('Error deleting crane:', error);
//     showNotification('Error deleting crane. It may be referenced by existing bookings.', 'danger');
//   }
// }

// // Show a notification toast
// function showNotification(message, type = 'success') {
//   const toastElement = document.getElementById('notificationToast');
//   const toastBody = toastElement.querySelector('.toast-body');

//   // Set the message
//   toastBody.textContent = message;

//   // Set the toast color based on type
//   toastElement.className = `toast align-items-center text-white bg-${type} border-0`;

//   // Show the toast
//   const toastInstance = bootstrap.Toast.getOrCreateInstance(toastElement);
//   toastInstance.show();
// }

// // Format date as DD-MM-YYYY
// function formatDate(date) {
//   return date.toLocaleDateString('en-GB', {
//     day: '2-digit',
//     month: '2-digit',
//     year: 'numeric'
//   }).replace(/\//g, '-');
// }

// // Format date and time as DD-MM-YYYY HH:MM
// function formatDateTime(date) {
//   return date.toLocaleDateString('en-GB', {
//     day: '2-digit',
//     month: '2-digit',
//     year: 'numeric',
//     hour: '2-digit',
//     minute: '2-digit'
//   }).replace(/\//g, '-');
// }

// // UI helper functions
// function showLoading() {
//   loadingIndicator.style.display = 'block';
//   craneTableContainer.style.display = 'none';
//   noDataMessage.style.display = 'none';
//   errorMessage.style.display = 'none';
// }

// function showTable() {
//   loadingIndicator.style.display = 'none';
//   craneTableContainer.style.display = 'block';
//   noDataMessage.style.display = 'none';
//   errorMessage.style.display = 'none';
// }

// function showNoData() {
//   loadingIndicator.style.display = 'none';
//   craneTableContainer.style.display = 'none';
//   noDataMessage.style.display = 'block';
//   errorMessage.style.display = 'none';
// }

// function showError() {
//   loadingIndicator.style.display = 'none';
//   craneTableContainer.style.display = 'none';
//   noDataMessage.style.display = 'none';
//   errorMessage.style.display = 'block';
// }

// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const cranesContainer = document.getElementById('cranesContainer');
const craneCards = document.getElementById('craneCards');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Modals
const addCraneModal = new bootstrap.Modal(document.getElementById('addCraneModal'));
const editCraneModal = new bootstrap.Modal(document.getElementById('editCraneModal'));
const deleteCraneModal = new bootstrap.Modal(document.getElementById('deleteCraneModal'));
const maintenanceLogModal = new bootstrap.Modal(document.getElementById('maintenanceLogModal'));
const uploadImageModal = new bootstrap.Modal(document.getElementById('uploadImageModal'));

// Form Elements
const addCraneForm = document.getElementById('addCraneForm');
const editCraneForm = document.getElementById('editCraneForm');
const addCraneStatus = document.getElementById('addCraneStatus');
const editCraneStatus = document.getElementById('editCraneStatus');
const addMaintenanceFields = document.getElementById('addMaintenanceFields');
const editMaintenanceFields = document.getElementById('editMaintenanceFields');
const uploadImageForm = document.getElementById('uploadImageForm');

// Buttons
const saveNewCraneBtn = document.getElementById('saveNewCraneBtn');
const updateCraneBtn = document.getElementById('updateCraneBtn');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
const saveImageBtn = document.getElementById('saveImageBtn');

// Toast
const notificationToast = document.getElementById('notificationToast');
const toast = new bootstrap.Toast(notificationToast);

// Global variables
let cranes = [];
let currentCraneId = null;

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
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

    renderCraneCards();
    showCranes();
  } catch (error) {
    console.error('Error loading cranes:', error);
    showError();
  }
}

// Check if status is "Available" or 0
function isAvailableStatus(status) {
  return status === 0 || status === 'Available';
}

// Render the crane cards
function renderCraneCards() {
  craneCards.innerHTML = '';

  cranes.forEach(crane => {
    const isAvailable = isAvailableStatus(crane.status);
    const statusClass = isAvailable ? 'status-available' : 'status-maintenance';
    const statusText = isAvailable ? 'Available' : 'Maintenance';

    const card = document.createElement('div');
    card.className = 'col-md-6 col-lg-4 mb-4';

    card.innerHTML = `
      <div class="card crane-card h-100">
        <div class="crane-image-container">
          ${
            crane.imagePath
              ? `<img src="${crane.imagePath}" alt="${crane.code}" class="crane-image">`
              : `<div class="crane-image-placeholder">
                <i class="bx bx-camera-off" style="font-size: 3rem;"></i>
              </div>`
          }
          <div class="upload-image-btn" data-id="${crane.id}" title="Upload Image">
            <i class="bx bx-camera"></i>
          </div>
        </div>
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-start mb-3">
            <h5 class="card-title mb-0">${crane.code}</h5>
            <span class="status-badge ${statusClass}">
              <i class="bx ${isAvailable ? 'bx-check' : 'bx-x'} me-1"></i>
              ${statusText}
            </span>
          </div>
          <p class="card-text mb-2">
            <strong>Capacity:</strong> ${crane.capacity} ton
          </p>
          <div id="maintenance-info-${crane.id}" class="text-muted small mb-3">
            ${!isAvailable ? '<span>Loading maintenance info...</span>' : ''}
          </div>
          <div class="d-flex justify-content-between align-items-center mt-auto">
            <div>
              <button type="button" class="btn btn-sm btn-outline-primary me-1 edit-crane-btn"
                      data-id="${crane.id}" title="Edit Crane">
                <i class="bx bx-edit"></i>
              </button>
              <button type="button" class="btn btn-sm btn-outline-danger me-1 delete-crane-btn"
                      data-id="${crane.id}" data-code="${crane.code}" title="Delete Crane">
                <i class="bx bx-trash"></i>
              </button>
              <button type="button" class="btn btn-sm btn-outline-info maintenance-log-btn"
                      data-id="${crane.id}" data-code="${crane.code}" title="Maintenance Logs">
                <i class="bx bx-history"></i>
              </button>
            </div>
          </div>
        </div>
      </div>
    `;

    craneCards.appendChild(card);

    // If crane is in maintenance, fetch maintenance info
    if (!isAvailable) {
      fetchMaintenanceInfo(crane.id);
    }
  });

  // Add event listeners to buttons
  document.querySelectorAll('.edit-crane-btn').forEach(btn => {
    btn.addEventListener('click', function () {
      const craneId = parseInt(this.dataset.id);
      openEditCraneModal(craneId);
    });
  });

  document.querySelectorAll('.delete-crane-btn').forEach(btn => {
    btn.addEventListener('click', function () {
      const craneId = parseInt(this.dataset.id);
      const craneCode = this.dataset.code;
      openDeleteModal(craneId, craneCode);
    });
  });

  document.querySelectorAll('.maintenance-log-btn').forEach(btn => {
    btn.addEventListener('click', function () {
      const craneId = parseInt(this.dataset.id);
      const craneCode = this.dataset.code;
      openMaintenanceLogModal(craneId, craneCode);
    });
  });

  document.querySelectorAll('.upload-image-btn').forEach(btn => {
    btn.addEventListener('click', function () {
      const craneId = parseInt(this.dataset.id);
      openUploadImageModal(craneId);
    });
  });
}

async function fetchMaintenanceInfo(craneId) {
  try {
    const response = await fetch(`/api/Cranes/${craneId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch crane details');
    }

    const craneDetail = await response.json();
    const maintenanceInfoEl = document.getElementById(`maintenance-info-${craneId}`);

    if (!maintenanceInfoEl) return;

    // If there are urgent logs
    if (craneDetail.urgentLogs && craneDetail.urgentLogs.length > 0) {
      const latestLog = craneDetail.urgentLogs[0]; // Assuming logs are ordered by date

      // Format dates
      const startDate = new Date(latestLog.urgentStartTime);
      const estimatedEndDate = new Date(latestLog.urgentEndTime);
      const actualEndDate = latestLog.actualUrgentEndTime ? new Date(latestLog.actualUrgentEndTime) : null;

      const startFormatted = formatDate(startDate);
      const endFormatted = formatDate(estimatedEndDate);

      // Calculate remaining time or show actual end time
      let timeInfo = '';

      if (actualEndDate) {
        // Show actual end time if exists
        const actualEndFormatted = formatDate(actualEndDate);
        timeInfo = `<div class="maintenance-timer text-success">Actual End: ${actualEndFormatted}</div>`;
      } else {
        // Calculate remaining time
        const now = new Date();
        if (estimatedEndDate > now) {
          const remainingMs = estimatedEndDate - now;
          const remainingDays = Math.floor(remainingMs / (1000 * 60 * 60 * 24));
          const remainingHours = Math.floor((remainingMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
          const remainingMinutes = Math.floor((remainingMs % (1000 * 60 * 60)) / (1000 * 60));

          timeInfo = `<div class="maintenance-timer">Remaining: ${remainingDays}d ${remainingHours}h ${remainingMinutes}m</div>`;
        } else {
          timeInfo = `<div class="maintenance-timer text-warning">Estimated time elapsed</div>`;
        }
      }

      maintenanceInfoEl.innerHTML = `
        <div><strong>Maintenance:</strong> ${startFormatted} - ${endFormatted}</div>
        ${timeInfo}
        <div class="text-truncate" title="${latestLog.reasons}">
          <small>${latestLog.reasons}</small>
        </div>
      `;
    } else {
      maintenanceInfoEl.innerHTML = `<span class="text-muted">No maintenance logs</span>`;
    }
  } catch (error) {
    console.error('Error fetching maintenance info:', error);
    const maintenanceInfoEl = document.getElementById(`maintenance-info-${craneId}`);
    if (maintenanceInfoEl) {
      maintenanceInfoEl.innerHTML = `<span class="text-danger">Error loading info</span>`;
    }
  }
}

// Setup event listeners
function setupEventListeners() {
  // Add crane form status change
  addCraneStatus.addEventListener('change', function () {
    addMaintenanceFields.style.display = this.value === '1' ? 'block' : 'none';
  });

  // Edit crane form status change
  editCraneStatus.addEventListener('change', function () {
    editMaintenanceFields.style.display = this.value === '1' ? 'block' : 'none';
  });

  // Save new crane button
  saveNewCraneBtn.addEventListener('click', saveNewCrane);

  // Update crane button
  updateCraneBtn.addEventListener('click', updateCrane);

  // Confirm delete button
  confirmDeleteBtn.addEventListener('click', deleteCrane);

  // Save image button
  saveImageBtn.addEventListener('click', uploadCraneImage);

  // Reset form when modals are closed
  document.getElementById('addCraneModal').addEventListener('hidden.bs.modal', function () {
    addCraneForm.reset();
    addMaintenanceFields.style.display = 'none';
    clearValidationErrors(addCraneForm);
  });

  document.getElementById('editCraneModal').addEventListener('hidden.bs.modal', function () {
    editCraneForm.reset();
    editMaintenanceFields.style.display = 'none';
    clearValidationErrors(editCraneForm);
    document.getElementById('editCraneImagePreview').style.display = 'none';
  });

  document.getElementById('uploadImageModal').addEventListener('hidden.bs.modal', function () {
    uploadImageForm.reset();
    clearValidationErrors(uploadImageForm);
    document.getElementById('currentImageContainer').style.display = 'none';
  });

  // Show image preview when file selected in edit form
  document.getElementById('editCraneImage').addEventListener('change', function (e) {
    if (this.files && this.files[0]) {
      const reader = new FileReader();
      const imgPreview = document.querySelector('#editCraneImagePreview img');

      reader.onload = function (e) {
        imgPreview.src = e.target.result;
        document.getElementById('editCraneImagePreview').style.display = 'block';
      };

      reader.readAsDataURL(this.files[0]);
    }
  });

  // Show image preview when file selected in upload form
  document.getElementById('craneImage').addEventListener('change', function (e) {
    if (this.files && this.files[0]) {
      const reader = new FileReader();
      const imgPreview = document.getElementById('currentImage');

      reader.onload = function (e) {
        imgPreview.src = e.target.result;
        document.getElementById('currentImageContainer').style.display = 'block';
      };

      reader.readAsDataURL(this.files[0]);
    }
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

  // Show current image if exists
  if (crane.imagePath) {
    const imgPreview = document.querySelector('#editCraneImagePreview img');
    imgPreview.src = crane.imagePath;
    document.getElementById('editCraneImagePreview').style.display = 'block';
  } else {
    document.getElementById('editCraneImagePreview').style.display = 'none';
  }

  // Show the modal
  editCraneModal.show();
}

// Open upload image modal
function openUploadImageModal(craneId) {
  const crane = cranes.find(c => c.id === craneId);

  if (!crane) {
    showNotification('Error finding crane data', 'danger');
    return;
  }

  document.getElementById('uploadImageCraneId').value = craneId;

  // Show current image if exists
  if (crane.imagePath) {
    document.getElementById('currentImage').src = crane.imagePath;
    document.getElementById('currentImageContainer').style.display = 'block';
  } else {
    document.getElementById('currentImageContainer').style.display = 'none';
  }

  // Clear any previous file selection
  document.getElementById('craneImage').value = '';
  clearValidationErrors(uploadImageForm);

  uploadImageModal.show();
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
    console.log('Maintenance logs received:', logs);

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
      const actualEndDate = log.actualUrgentEndTime ? new Date(log.actualUrgentEndTime) : null;

      const row = document.createElement('tr');

      // Create each cell individually to ensure correct order
      // Cell 1: Start Date
      const startDateCell = document.createElement('td');
      startDateCell.textContent = formatDateTime(startDate);
      row.appendChild(startDateCell);

      // Cell 2: Estimated End Date
      const endDateCell = document.createElement('td');
      endDateCell.textContent = formatDateTime(endDate);
      row.appendChild(endDateCell);

      // Cell 3: Duration
      const durationCell = document.createElement('td');
      durationCell.textContent = `${log.estimatedUrgentDays} days, ${log.estimatedUrgentHours} hours`;
      row.appendChild(durationCell);

      // Cell 4: Actual End Date
      const actualEndDateCell = document.createElement('td');
      actualEndDateCell.textContent = actualEndDate ? formatDateTime(actualEndDate) : 'N/A';
      row.appendChild(actualEndDateCell);

      // Cell 5: Reasons
      const reasonsCell = document.createElement('td');
      reasonsCell.textContent = log.reasons || 'No reason provided';
      row.appendChild(reasonsCell);

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
  const imageFile = document.getElementById('addCraneImage').files[0];

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

  // Create FormData object for multipart/form-data (including file)
  const formData = new FormData();
  formData.append('Code', code);
  formData.append('Capacity', capacity);
  formData.append('Status', status);

  // Add image if selected
  if (imageFile) {
    formData.append('Image', imageFile);
  }

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
    formData.append('UrgentLog.UrgentStartTime', new Date().toISOString());
    formData.append('UrgentLog.EstimatedUrgentDays', days);
    formData.append('UrgentLog.EstimatedUrgentHours', hours);
    formData.append('UrgentLog.Reasons', reasons);
  }

  try {
    // Send request to API
    const response = await fetch('/api/Cranes', {
      method: 'POST',
      body: formData
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
  const imageFile = document.getElementById('editCraneImage').files[0];
  const removeImage = document.getElementById('removeExistingImage')?.checked || false;

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

  // Create FormData object for multipart/form-data (including file)
  const formData = new FormData();
  formData.append('Crane.Code', code);
  formData.append('Crane.Capacity', capacity);
  formData.append('Crane.Status', status);

  // Add image if selected
  if (imageFile) {
    formData.append('Crane.Image', imageFile);
  }

  // Flag to remove existing image
  if (removeImage) {
    formData.append('removeImage', 'true');
  }

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

      // Add UrgentLog data - PERBAIKAN DI SINI
      formData.append('UrgentLog.UrgentStartTime', new Date().toISOString());
      formData.append('UrgentLog.EstimatedUrgentDays', days);
      formData.append('UrgentLog.EstimatedUrgentHours', hours);
      formData.append('UrgentLog.Reasons', reasons);
    }
  }

  try {
    // Send request to API
    const response = await fetch(`/api/Cranes/${craneId}`, {
      method: 'PUT',
      body: formData
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

// Upload crane image
async function uploadCraneImage() {
  const craneId = document.getElementById('uploadImageCraneId').value;
  const imageFile = document.getElementById('craneImage').files[0];

  // Validate form
  if (!imageFile) {
    document.getElementById('craneImage').classList.add('is-invalid');
    return;
  }

  // Create FormData object
  const formData = new FormData();
  formData.append('image', imageFile);

  try {
    // Send request to API
    const response = await fetch(`/api/Cranes/${craneId}/image`, {
      method: 'POST',
      body: formData
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Error response:', errorText);
      throw new Error('Failed to upload image');
    }

    uploadImageModal.hide();
    showNotification('Image uploaded successfully', 'success');
    loadCranes();
  } catch (error) {
    console.error('Error uploading image:', error);
    showNotification('Error uploading image', 'danger');
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
  return date
    .toLocaleDateString('en-GB', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    })
    .replace(/\//g, '-');
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
  cranesContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showCranes() {
  loadingIndicator.style.display = 'none';
  cranesContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  cranesContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  cranesContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
