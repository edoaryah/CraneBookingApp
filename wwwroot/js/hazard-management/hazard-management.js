// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const hazardTableContainer = document.getElementById('hazardTableContainer');
const hazardTableBody = document.getElementById('hazardTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Modal elements
const createHazardModal = document.getElementById('createHazardModal');
const editHazardModal = document.getElementById('editHazardModal');
const deleteHazardModal = document.getElementById('deleteHazardModal');

// Form elements
const createHazardForm = document.getElementById('createHazardForm');
const editHazardForm = document.getElementById('editHazardForm');
const deleteHazardId = document.getElementById('deleteHazardId');

// Buttons
const saveHazardBtn = document.getElementById('saveHazardBtn');
const updateHazardBtn = document.getElementById('updateHazardBtn');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

// Global variables
let hazards = [];

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
  loadHazards();
  initializeEventListeners();
});

// Function to load hazards
async function loadHazards() {
  showLoading();

  try {
    const response = await fetch('/api/Hazards');

    if (!response.ok) {
      throw new Error('Failed to fetch data');
    }

    hazards = await response.json();

    if (hazards.length === 0) {
      showNoData();
      return;
    }

    // Sort hazards by name
    hazards.sort((a, b) => a.name.localeCompare(b.name));

    // Render the table
    renderHazardTable();
    showTable();
  } catch (error) {
    console.error('Error loading hazards:', error);
    showError();
  }
}

// Render the hazards table
function renderHazardTable() {
  hazardTableBody.innerHTML = '';

  hazards.forEach(hazard => {
    const row = document.createElement('tr');

    // Populate row
    row.innerHTML = `
      <td>${hazard.id}</td>
      <td>${hazard.name}</td>
      <td>
        <div class="dropdown">
          <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
            <i class="bx bx-dots-vertical-rounded"></i>
          </button>
          <div class="dropdown-menu">
            <a class="dropdown-item edit-hazard" href="javascript:void(0);" data-id="${hazard.id}">
              <i class="bx bx-edit-alt me-1"></i> Edit
            </a>
            <a class="dropdown-item delete-hazard" href="javascript:void(0);" data-id="${hazard.id}">
              <i class="bx bx-trash me-1"></i> Delete
            </a>
          </div>
        </div>
      </td>
    `;

    hazardTableBody.appendChild(row);
  });

  // Add event listeners to the newly created buttons
  document.querySelectorAll('.edit-hazard').forEach(btn => {
    btn.addEventListener('click', handleEditClick);
  });

  document.querySelectorAll('.delete-hazard').forEach(btn => {
    btn.addEventListener('click', handleDeleteClick);
  });
}

// Initialize event listeners
function initializeEventListeners() {
  // Create hazard
  saveHazardBtn.addEventListener('click', handleCreateHazard);

  // Update hazard
  updateHazardBtn.addEventListener('click', handleUpdateHazard);

  // Delete hazard
  confirmDeleteBtn.addEventListener('click', handleDeleteHazard);

  // Reset form on modal close
  createHazardModal.addEventListener('hidden.bs.modal', () => {
    createHazardForm.reset();
  });
}

// Event handler for the edit button
function handleEditClick(e) {
  const hazardId = parseInt(e.currentTarget.dataset.id);
  const hazard = hazards.find(h => h.id === hazardId);

  if (hazard) {
    // Populate the edit form
    document.getElementById('editHazardId').value = hazard.id;
    document.getElementById('editHazardName').value = hazard.name;

    // Show the modal
    const modal = new bootstrap.Modal(editHazardModal);
    modal.show();
  }
}

// Event handler for the delete button
function handleDeleteClick(e) {
  const hazardId = parseInt(e.currentTarget.dataset.id);
  deleteHazardId.value = hazardId;

  // Show the modal
  const modal = new bootstrap.Modal(deleteHazardModal);
  modal.show();
}

// Create a new hazard
async function handleCreateHazard() {
  // Disable the button during operation
  saveHazardBtn.disabled = true;
  saveHazardBtn.innerHTML =
    '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...';

  try {
    const formData = new FormData(createHazardForm);
    const hazardData = {
      name: formData.get('name')
    };

    const response = await fetch('/api/Hazards', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(hazardData)
    });

    if (!response.ok) {
      throw new Error('Failed to create hazard');
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(createHazardModal);
    modal.hide();

    // Reload the data
    await loadHazards();

    // Show success message
    showToast('Hazard created successfully!', 'success');
  } catch (error) {
    console.error('Error creating hazard:', error);
    showToast('Failed to create hazard', 'error');
  } finally {
    // Always re-enable the button
    saveHazardBtn.disabled = false;
    saveHazardBtn.innerHTML = 'Save';
  }
}

// Update an existing hazard
async function handleUpdateHazard() {
  // Disable the button during operation
  updateHazardBtn.disabled = true;
  updateHazardBtn.innerHTML =
    '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Updating...';

  try {
    const formData = new FormData(editHazardForm);
    const hazardId = parseInt(formData.get('id'));

    const hazardData = {
      name: formData.get('name')
    };

    const response = await fetch(`/api/Hazards/${hazardId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(hazardData)
    });

    if (!response.ok) {
      throw new Error('Failed to update hazard');
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(editHazardModal);
    modal.hide();

    // Reload the data
    await loadHazards();

    // Show success message
    showToast('Hazard updated successfully!', 'success');
  } catch (error) {
    console.error('Error updating hazard:', error);
    showToast('Failed to update hazard', 'error');
  } finally {
    // Always re-enable the button
    updateHazardBtn.disabled = false;
    updateHazardBtn.innerHTML = 'Update';
  }
}

// Delete a hazard
async function handleDeleteHazard() {
  // Disable the button during operation
  confirmDeleteBtn.disabled = true;
  confirmDeleteBtn.innerHTML =
    '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Deleting...';

  try {
    const hazardId = parseInt(deleteHazardId.value);

    const response = await fetch(`/api/Hazards/${hazardId}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      throw new Error('Failed to delete hazard');
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(deleteHazardModal);
    modal.hide();

    // Reload the data
    await loadHazards();

    // Show success message
    showToast('Hazard deleted successfully!', 'success');
  } catch (error) {
    console.error('Error deleting hazard:', error);
    showToast('Failed to delete hazard', 'error');
  } finally {
    // Always re-enable the button
    confirmDeleteBtn.disabled = false;
    confirmDeleteBtn.innerHTML = 'Delete';
  }
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
  hazardTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  hazardTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  hazardTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  hazardTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
