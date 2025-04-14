// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const rolesContainer = document.getElementById('rolesContainer');
const roleCards = document.getElementById('roleCards');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');
const createRoleBtn = document.getElementById('createRoleBtn');
const roleModal = new bootstrap.Modal(document.getElementById('roleModal'));
const roleModalLabel = document.getElementById('roleModalLabel');
const roleForm = document.getElementById('roleForm');
const roleId = document.getElementById('roleId');
const roleName = document.getElementById('roleName');
const roleNameError = document.getElementById('roleNameError');
const roleDescription = document.getElementById('roleDescription');
const roleIsActive = document.getElementById('roleIsActive');
const saveRoleBtn = document.getElementById('saveRoleBtn');
const deleteRoleModal = new bootstrap.Modal(document.getElementById('deleteRoleModal'));
const deleteRoleName = document.getElementById('deleteRoleName');
const deleteErrorMessage = document.getElementById('deleteErrorMessage');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

// Global variables
let roles = [];

// Initialize the page
document.addEventListener('DOMContentLoaded', function () {
  loadRoles();

  // Event listeners
  createRoleBtn.addEventListener('click', showCreateRoleModal);
  saveRoleBtn.addEventListener('click', saveRole);
  confirmDeleteBtn.addEventListener('click', deleteRole);
});

// Load roles
async function loadRoles() {
  showLoading();

  try {
    const response = await fetch('/api/Roles');

    if (!response.ok) {
      throw new Error('Failed to fetch roles');
    }

    roles = await response.json();

    if (roles.length === 0) {
      showNoData();
      return;
    }

    renderRoleCards();
    showRoles();
  } catch (error) {
    console.error('Error loading roles:', error);
    showError();
  }
}

// Render role cards
function renderRoleCards() {
  roleCards.innerHTML = '';

  // Sort roles - active first, then by name
  roles.sort((a, b) => {
    if (a.isActive !== b.isActive) {
      return b.isActive - a.isActive; // Active roles first
    }
    return a.name.localeCompare(b.name); // Then alphabetically
  });

  roles.forEach(role => {
    const card = document.createElement('div');
    card.className = 'col-md-6 col-lg-4 mb-4';

    card.innerHTML = `
            <div class="card role-card h-100">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <h5 class="card-title mb-0">${role.name}</h5>
                        <span class="status-badge ${role.isActive ? 'active' : 'inactive'}">
                            <i class="bx ${role.isActive ? 'bx-check' : 'bx-x'} me-1"></i>
                            ${role.isActive ? 'Active' : 'Inactive'}
                        </span>
                    </div>
                    <p class="card-text text-muted mb-3" style="min-height: 3em;">
                        ${role.description || 'No description provided'}
                    </p>
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="user-count">
                            <i class="bx bx-user me-1"></i>
                            ${role.userCount} user${role.userCount !== 1 ? 's' : ''}
                        </span>
                        <div>
                            <a href="/RoleManagement/Users/${role.id}" class="btn btn-sm btn-outline-primary me-1" title="Manage Users">
                                <i class="bx bx-user-plus"></i>
                            </a>
                            <button type="button" class="btn btn-sm btn-outline-secondary me-1 edit-role-btn"
                                    data-id="${role.id}" title="Edit Role">
                                <i class="bx bx-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-danger delete-role-btn"
                                    data-id="${role.id}" data-name="${role.name}" title="Delete Role"
                                    ${role.userCount > 0 ? 'disabled' : ''}>
                                <i class="bx bx-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

    roleCards.appendChild(card);
  });

  // Add event listeners to edit and delete buttons
  document.querySelectorAll('.edit-role-btn').forEach(button => {
    button.addEventListener('click', function () {
      const roleId = this.getAttribute('data-id');
      showEditRoleModal(roleId);
    });
  });

  document.querySelectorAll('.delete-role-btn').forEach(button => {
    button.addEventListener('click', function () {
      if (!this.disabled) {
        const roleId = this.getAttribute('data-id');
        const roleName = this.getAttribute('data-name');
        showDeleteRoleModal(roleId, roleName);
      }
    });
  });
}

// Show create role modal
function showCreateRoleModal() {
  // Reset form
  roleForm.reset();
  roleId.value = '';
  roleName.classList.remove('is-invalid');
  roleNameError.textContent = '';

  // Update modal title
  roleModalLabel.textContent = 'Create New Role';

  // Show modal
  roleModal.show();
}

// Show edit role modal
function showEditRoleModal(id) {
  // Find role
  const role = roles.find(r => r.id == id);
  if (!role) return;

  // Reset validation
  roleName.classList.remove('is-invalid');
  roleNameError.textContent = '';

  // Fill form
  roleId.value = role.id;
  roleName.value = role.name;
  roleDescription.value = role.description || '';
  roleIsActive.checked = role.isActive;

  // Update modal title
  roleModalLabel.textContent = 'Edit Role';

  // Show modal
  roleModal.show();
}

// Show delete role modal
function showDeleteRoleModal(id, name) {
  roleId.value = id;
  deleteRoleName.textContent = name;
  deleteErrorMessage.style.display = 'none';

  deleteRoleModal.show();
}

// Save role (create or update)
async function saveRole() {
  // Validate form
  if (!validateRoleForm()) {
    return;
  }

  const isEdit = roleId.value !== '';
  const url = isEdit ? `/api/Roles/${roleId.value}` : '/api/Roles';
  const method = isEdit ? 'PUT' : 'POST';

  // Prepare data
  const data = {
    name: roleName.value.trim(),
    description: roleDescription.value.trim() || null,
    isActive: roleIsActive.checked
  };

  try {
    const response = await fetch(url, {
      method: method,
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to save role');
    }

    // Hide modal and reload roles
    roleModal.hide();
    await loadRoles();
  } catch (error) {
    console.error('Error saving role:', error);

    // Show error in form
    roleName.classList.add('is-invalid');
    roleNameError.textContent = error.message || 'An error occurred';
  }
}

// Delete role
async function deleteRole() {
  const id = roleId.value;
  if (!id) return;

  try {
    const response = await fetch(`/api/Roles/${id}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to delete role');
    }

    // Hide modal and reload roles
    deleteRoleModal.hide();
    await loadRoles();
  } catch (error) {
    console.error('Error deleting role:', error);

    // Show error in modal
    deleteErrorMessage.textContent = error.message || 'An error occurred';
    deleteErrorMessage.style.display = 'block';
  }
}

// Validate role form
function validateRoleForm() {
  let isValid = true;

  // Validate role name
  if (!roleName.value.trim()) {
    roleName.classList.add('is-invalid');
    roleNameError.textContent = 'Role name is required';
    isValid = false;
  } else {
    roleName.classList.remove('is-invalid');
    roleNameError.textContent = '';
  }

  return isValid;
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  rolesContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showRoles() {
  loadingIndicator.style.display = 'none';
  rolesContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  rolesContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  rolesContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
