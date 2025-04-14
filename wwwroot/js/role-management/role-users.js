// DOM Elements - Main Page
const loadingIndicator = document.getElementById('loadingIndicator');
const userTableContainer = document.getElementById('userTableContainer');
const userTableBody = document.getElementById('userTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');
const roleTitle = document.getElementById('roleTitle');
const roleDescription = document.getElementById('roleDescription');
const addUserBtn = document.getElementById('addUserBtn');

// DOM Elements - Add User Modal
const addUserModal = new bootstrap.Modal(document.getElementById('addUserModal'));
const modalLoadingIndicator = document.getElementById('modalLoadingIndicator');
const modalErrorMessage = document.getElementById('modalErrorMessage');
const addUserForm = document.getElementById('addUserForm');
const departmentFilter = document.getElementById('departmentFilter');
const userSelect = document.getElementById('userSelect');
const userNotes = document.getElementById('userNotes');
const noUsersMessage = document.getElementById('noUsersMessage');
const saveUserBtn = document.getElementById('saveUserBtn');

// DOM Elements - Edit User Modal
const editUserModal = new bootstrap.Modal(document.getElementById('editUserModal'));
const editUserRoleId = document.getElementById('editUserRoleId');
const editUserName = document.getElementById('editUserName');
const editUserNotes = document.getElementById('editUserNotes');
const updateUserBtn = document.getElementById('updateUserBtn');

// DOM Elements - Remove User Modal
const removeUserModal = new bootstrap.Modal(document.getElementById('removeUserModal'));
const removeUserName = document.getElementById('removeUserName');
const confirmRemoveBtn = document.getElementById('confirmRemoveBtn');

// Global variables
let currentRole = null;
let users = [];
let availableUsers = [];
let departments = new Set();
let userRoleToRemove = null;

// Initialize page
function loadRoleAndUsers(roleId) {
  if (!roleId) {
    showError('Invalid role ID');
    return;
  }

  // Load role and its users
  loadRole(roleId)
    .then(() => {
      loadUsers(roleId);
    })
    .catch(error => {
      console.error('Error initializing page:', error);
      showError('Failed to load role information');
    });

  // Event listeners
  addUserBtn.addEventListener('click', () => showAddUserModal(roleId));
  saveUserBtn.addEventListener('click', saveUser);
  updateUserBtn.addEventListener('click', updateUser);
  confirmRemoveBtn.addEventListener('click', removeUser);
  departmentFilter.addEventListener('change', filterUsers);
}

// Load role information
async function loadRole(roleId) {
  try {
    const response = await fetch(`/api/Roles/${roleId}`);

    if (!response.ok) {
      throw new Error('Failed to fetch role');
    }

    currentRole = await response.json();

    // Update page title and description
    roleTitle.textContent = `${currentRole.name} - Users`;
    roleDescription.textContent = currentRole.description || '';

    document.title = `${currentRole.name} - Role Users`;
  } catch (error) {
    console.error('Error loading role:', error);
    throw error;
  }
}

// Load users with this role
async function loadUsers(roleId) {
  showLoading();

  try {
    const response = await fetch(`/api/Roles/${roleId}/Users`);

    if (!response.ok) {
      throw new Error('Failed to fetch users');
    }

    users = await response.json();

    if (users.length === 0) {
      showNoData();
      return;
    }

    renderUserTable();
    showTable();
  } catch (error) {
    console.error('Error loading users:', error);
    showError('Failed to load users');
  }
}

// Render user table
function renderUserTable() {
  userTableBody.innerHTML = '';

  // Sort users by name
  users.sort((a, b) => {
    return (a.employeeName || a.ldapUser).localeCompare(b.employeeName || b.ldapUser);
  });

  users.forEach(user => {
    const row = document.createElement('tr');

    // Get user initials for avatar
    const name = user.employeeName || user.ldapUser;
    const initials = name
      .split(' ')
      .map(part => part.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();

    row.innerHTML = `
            <td>
                <div class="d-flex align-items-center">
                    <div class="user-avatar">${initials}</div>
                    <div>
                        <strong>${name}</strong>
                        <div class="text-muted small">${user.ldapUser}</div>
                    </div>
                </div>
            </td>
            <td>${user.department || 'N/A'}</td>
            <td>${user.position || 'N/A'}</td>
            <td>${user.notes || '-'}</td>
            <td>
                <div class="dropdown">
                    <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
                        <i class="bx bx-dots-vertical-rounded"></i>
                    </button>
                    <div class="dropdown-menu">
                        <a class="dropdown-item edit-user-btn" href="javascript:void(0);"
                           data-id="${user.id}" data-name="${name}">
                            <i class="bx bx-edit-alt me-1"></i> Edit Notes
                        </a>
                        <a class="dropdown-item text-danger remove-user-btn" href="javascript:void(0);"
                           data-id="${user.id}" data-name="${name}">
                            <i class="bx bx-trash me-1"></i> Remove from Role
                        </a>
                    </div>
                </div>
            </td>
        `;

    userTableBody.appendChild(row);
  });

  // Add event listeners to buttons
  document.querySelectorAll('.edit-user-btn').forEach(button => {
    button.addEventListener('click', function () {
      const id = this.getAttribute('data-id');
      const name = this.getAttribute('data-name');
      showEditUserModal(id, name);
    });
  });

  document.querySelectorAll('.remove-user-btn').forEach(button => {
    button.addEventListener('click', function () {
      const id = this.getAttribute('data-id');
      const name = this.getAttribute('data-name');
      showRemoveUserModal(id, name);
    });
  });
}

// Show add user modal
async function showAddUserModal(roleId) {
  // Reset form
  addUserForm.reset();
  departmentFilter.innerHTML = '<option value="">All Departments</option>';
  userSelect.innerHTML = '<option value="">Select a user</option>';

  // Show loading, hide form and error
  modalLoadingIndicator.style.display = 'block';
  addUserForm.style.display = 'none';
  modalErrorMessage.style.display = 'none';
  noUsersMessage.style.display = 'none';

  addUserModal.show();

  try {
    // Load available users for this role
    const response = await fetch(`/api/Roles/${roleId}/AvailableEmployees`);

    if (!response.ok) {
      throw new Error('Failed to fetch available users');
    }

    availableUsers = await response.json();

    if (availableUsers.length === 0) {
      // No eligible users found
      modalLoadingIndicator.style.display = 'none';
      noUsersMessage.style.display = 'block';
      return;
    }

    // Get unique departments
    departments = new Set();
    availableUsers.forEach(user => {
      if (user.department) {
        departments.add(user.department);
      }
    });

    // Populate department filter
    departmentFilter.innerHTML = '<option value="">All Departments</option>';
    [...departments].sort().forEach(dept => {
      const option = document.createElement('option');
      option.value = dept;
      option.textContent = dept;
      departmentFilter.appendChild(option);
    });

    // Populate user select with all users
    populateUserSelect();

    // Show form
    modalLoadingIndicator.style.display = 'none';
    addUserForm.style.display = 'block';
  } catch (error) {
    console.error('Error loading available users:', error);
    modalLoadingIndicator.style.display = 'none';
    modalErrorMessage.style.display = 'block';
    modalErrorMessage.textContent = 'Failed to load available users. Please try again.';
  }
}

// Populate user select based on department filter
function populateUserSelect() {
  const selectedDepartment = departmentFilter.value;

  // Filter users by department if a department is selected
  const filteredUsers = selectedDepartment
    ? availableUsers.filter(user => user.department === selectedDepartment)
    : availableUsers;

  // Sort users by name
  filteredUsers.sort((a, b) => a.name.localeCompare(b.name));

  // Populate select
  userSelect.innerHTML = '<option value="">Select a user</option>';

  filteredUsers.forEach(user => {
    const option = document.createElement('option');
    option.value = user.ldapUser;
    option.textContent = `${user.name} (${user.ldapUser})`;
    option.setAttribute('data-department', user.department || '');
    option.setAttribute('data-position', user.positionTitle || '');
    userSelect.appendChild(option);
  });
}

// Filter users when department changes
function filterUsers() {
  populateUserSelect();
}

// Show edit user modal
function showEditUserModal(id, name) {
  const userRole = users.find(u => u.id == id);
  if (!userRole) return;

  editUserRoleId.value = userRole.id;
  editUserName.value = name;
  editUserNotes.value = userRole.notes || '';

  editUserModal.show();
}

// Show remove user modal
function showRemoveUserModal(id, name) {
  userRoleToRemove = id;
  removeUserName.textContent = name;

  removeUserModal.show();
}

// Save user (assign role to user)
async function saveUser() {
  // Validate form
  if (!userSelect.value) {
    alert('Please select a user');
    return;
  }

  // Create user role data
  const userRoleData = {
    ldapUser: userSelect.value,
    roleId: currentRole.id,
    notes: userNotes.value.trim() || null
  };

  try {
    // Save user role
    const response = await fetch('/api/Roles/AssignToUser', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(userRoleData)
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to assign role to user');
    }

    // Close modal and reload users
    addUserModal.hide();
    await loadUsers(currentRole.id);
  } catch (error) {
    console.error('Error assigning role to user:', error);
    modalErrorMessage.style.display = 'block';
    modalErrorMessage.textContent = error.message || 'Failed to assign role to user. Please try again.';
  }
}

// Update user notes
async function updateUser() {
  const id = editUserRoleId.value;
  if (!id) return;

  const updateData = {
    notes: editUserNotes.value.trim() || null
  };

  try {
    // Update user role
    const response = await fetch(`/api/Roles/UserRole/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updateData)
    });

    if (!response.ok) {
      throw new Error('Failed to update user notes');
    }

    // Close modal and reload users
    editUserModal.hide();
    await loadUsers(currentRole.id);
  } catch (error) {
    console.error('Error updating user notes:', error);
    alert('Failed to update user notes. Please try again.');
  }
}

// Remove user from role
async function removeUser() {
  if (!userRoleToRemove) return;

  try {
    // Remove user role
    const response = await fetch(`/api/Roles/UserRole/${userRoleToRemove}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      throw new Error('Failed to remove user from role');
    }

    // Close modal and reload users
    removeUserModal.hide();
    await loadUsers(currentRole.id);
  } catch (error) {
    console.error('Error removing user from role:', error);
    alert('Failed to remove user from role. Please try again.');
  } finally {
    userRoleToRemove = null;
  }
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  userTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  userTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  userTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError(message = 'An error occurred. Please try again later.') {
  loadingIndicator.style.display = 'none';
  userTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
  errorMessage.textContent = message;
}
