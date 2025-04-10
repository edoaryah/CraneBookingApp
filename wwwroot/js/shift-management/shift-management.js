// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const shiftTableContainer = document.getElementById('shiftTableContainer');
const shiftTableBody = document.getElementById('shiftTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Modal elements
const createShiftModal = document.getElementById('createShiftModal');
const editShiftModal = document.getElementById('editShiftModal');
const deleteShiftModal = document.getElementById('deleteShiftModal');

// Form elements
const createShiftForm = document.getElementById('createShiftForm');
const editShiftForm = document.getElementById('editShiftForm');
const deleteShiftId = document.getElementById('deleteShiftId');

// Buttons
const saveShiftBtn = document.getElementById('saveShiftBtn');
const updateShiftBtn = document.getElementById('updateShiftBtn');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

// Global variables
let shiftDefinitions = [];

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
  loadShiftDefinitions();
  initializeEventListeners();
});

// Function to load shift definitions
async function loadShiftDefinitions() {
  showLoading();

  try {
    const response = await fetch('/api/ShiftDefinitions');

    if (!response.ok) {
      throw new Error('Failed to fetch data');
    }

    shiftDefinitions = await response.json();

    if (shiftDefinitions.length === 0) {
      showNoData();
      return;
    }

    // Sort shifts by start time
    shiftDefinitions.sort((a, b) => a.startTime.localeCompare(b.startTime));

    // Render the table
    renderShiftTable();
    showTable();
  } catch (error) {
    console.error('Error loading shift definitions:', error);
    showError();
  }
}

// Render the shift definitions table
function renderShiftTable() {
  shiftTableBody.innerHTML = '';

  shiftDefinitions.forEach(shift => {
    const row = document.createElement('tr');

    // Format times
    const formattedStartTime = formatTime(shift.startTime);
    const formattedEndTime = formatTime(shift.endTime);

    // Create status badge
    const statusBadgeClass = shift.isActive ? 'bg-label-success' : 'bg-label-secondary';
    const statusText = shift.isActive ? 'Active' : 'Inactive';

    // Populate row
    row.innerHTML = `
      <td>${shift.id}</td>
      <td>${shift.name}</td>
      <td>${formattedStartTime}</td>
      <td>${formattedEndTime}</td>
      <td>${shift.category}</td>
      <td><span class="badge ${statusBadgeClass} me-1">${statusText}</span></td>
      <td>
        <div class="dropdown">
          <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
            <i class="bx bx-dots-vertical-rounded"></i>
          </button>
          <div class="dropdown-menu">
            <a class="dropdown-item edit-shift" href="javascript:void(0);" data-id="${shift.id}">
              <i class="bx bx-edit-alt me-1"></i> Edit
            </a>
            <a class="dropdown-item delete-shift" href="javascript:void(0);" data-id="${shift.id}">
              <i class="bx bx-trash me-1"></i> Delete
            </a>
          </div>
        </div>
      </td>
    `;

    shiftTableBody.appendChild(row);
  });

  // Add event listeners to the newly created buttons
  document.querySelectorAll('.edit-shift').forEach(btn => {
    btn.addEventListener('click', handleEditClick);
  });

  document.querySelectorAll('.delete-shift').forEach(btn => {
    btn.addEventListener('click', handleDeleteClick);
  });
}

// Initialize event listeners
function initializeEventListeners() {
  // Create shift
  saveShiftBtn.addEventListener('click', handleCreateShift);

  // Update shift
  updateShiftBtn.addEventListener('click', handleUpdateShift);

  // Delete shift
  confirmDeleteBtn.addEventListener('click', handleDeleteShift);

  // Reset form on modal close
  createShiftModal.addEventListener('hidden.bs.modal', () => {
    createShiftForm.reset();
  });
}

// Event handler for the edit button
function handleEditClick(e) {
  const shiftId = parseInt(e.currentTarget.dataset.id);
  const shift = shiftDefinitions.find(s => s.id === shiftId);

  if (shift) {
    // Populate the edit form
    document.getElementById('editShiftId').value = shift.id;
    document.getElementById('editShiftName').value = shift.name;
    document.getElementById('editStartTime').value = shift.startTime.substring(0, 5);
    document.getElementById('editEndTime').value = shift.endTime.substring(0, 5);
    document.getElementById('editCategory').value = shift.category;
    document.getElementById('editIsActive').checked = shift.isActive;

    // Show the modal
    const modal = new bootstrap.Modal(editShiftModal);
    modal.show();
  }
}

// Event handler for the delete button
function handleDeleteClick(e) {
  const shiftId = parseInt(e.currentTarget.dataset.id);
  deleteShiftId.value = shiftId;

  // Show the modal
  const modal = new bootstrap.Modal(deleteShiftModal);
  modal.show();
}

// Create a new shift definition
async function handleCreateShift() {
  try {
    const formData = new FormData(createShiftForm);
    const shiftData = {
      name: formData.get('name'),
      startTime: formData.get('startTime'),
      endTime: formData.get('endTime'),
      category: formData.get('category'),
      isActive: formData.get('isActive') === 'on'
    };

    const response = await fetch('/api/ShiftDefinitions', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(shiftData)
    });

    if (!response.ok) {
      throw new Error('Failed to create shift definition');
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(createShiftModal);
    modal.hide();

    // Reload the data
    await loadShiftDefinitions();

    // Show success message
    showToast('Shift definition created successfully!', 'success');
  } catch (error) {
    console.error('Error creating shift definition:', error);
    showToast('Failed to create shift definition', 'error');
  }
}

// Update an existing shift definition
async function handleUpdateShift() {
  try {
    const formData = new FormData(editShiftForm);
    const shiftId = parseInt(formData.get('id'));

    const shiftData = {
      name: formData.get('name'),
      startTime: formData.get('startTime'),
      endTime: formData.get('endTime'),
      category: formData.get('category'),
      isActive: formData.get('isActive') === 'on'
    };

    const response = await fetch(`/api/ShiftDefinitions/${shiftId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(shiftData)
    });

    if (!response.ok) {
      throw new Error('Failed to update shift definition');
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(editShiftModal);
    modal.hide();

    // Reload the data
    await loadShiftDefinitions();

    // Show success message
    showToast('Shift definition updated successfully!', 'success');
  } catch (error) {
    console.error('Error updating shift definition:', error);
    showToast('Failed to update shift definition', 'error');
  }
}

// Delete a shift definition
async function handleDeleteShift() {
  try {
    const shiftId = parseInt(deleteShiftId.value);

    const response = await fetch(`/api/ShiftDefinitions/${shiftId}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      throw new Error('Failed to delete shift definition');
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(deleteShiftModal);
    modal.hide();

    // Reload the data
    await loadShiftDefinitions();

    // Show success message
    showToast('Shift definition deleted successfully!', 'success');
  } catch (error) {
    console.error('Error deleting shift definition:', error);
    showToast('Failed to delete shift definition', 'error');
  }
}

// Format time from "HH:MM:SS" to "HH:MM AM/PM"
function formatTime(timeString) {
  if (!timeString) return '';

  const [hours, minutes] = timeString.split(':');
  const hour = parseInt(hours);

  // Handle 24-hour format
  const period = hour >= 12 ? 'PM' : 'AM';
  const hour12 = hour % 12 || 12; // Convert 0 to 12 for 12 AM

  return `${hour12}:${minutes} ${period}`;
}

// Show toast notification
function showToast(message, type = 'info') {
  // Check if we have Toastr library
  if (typeof toastr !== 'undefined') {
    toastr[type](message);
  } else {
    // Fallback to alert
    alert(message);
  }
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  shiftTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  shiftTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  shiftTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  shiftTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
