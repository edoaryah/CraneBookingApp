// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const subcategoryTableContainer = document.getElementById('subcategoryTableContainer');
const subcategoryTableBody = document.getElementById('subcategoryTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Modal elements
const createSubcategoryModal = document.getElementById('createSubcategoryModal');
const editSubcategoryModal = document.getElementById('editSubcategoryModal');
const deleteSubcategoryModal = document.getElementById('deleteSubcategoryModal');

// Form elements
const createSubcategoryForm = document.getElementById('createSubcategoryForm');
const editSubcategoryForm = document.getElementById('editSubcategoryForm');
const deleteSubcategoryId = document.getElementById('deleteSubcategoryId');

// Select elements
const categorySelect = document.getElementById('categorySelect');
const editCategorySelect = document.getElementById('editCategorySelect');

// Buttons
const saveSubcategoryBtn = document.getElementById('saveSubcategoryBtn');
const updateSubcategoryBtn = document.getElementById('updateSubcategoryBtn');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

// Global variables
let subcategories = [];
let categories = [];
let categoryMap = {}; // Maps numeric values to category names
let categoryColors = {}; // Will be populated based on category names

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
  loadCategories()
    .then(() => loadSubcategories())
    .catch(error => {
      console.error('Error during initialization:', error);
      showError();
    });

  initializeEventListeners();
});

// Function to load categories
async function loadCategories() {
  try {
    const response = await fetch('/api/UsageCategories');

    if (!response.ok) {
      throw new Error('Failed to fetch categories');
    }

    categories = await response.json();

    // Initialize categoryMap and categoryColors
    categories.forEach(category => {
      categoryMap[category.id] = category.name;

      // Assign colors based on category name
      switch (category.name) {
        case 'Operating':
          categoryColors[category.name] = 'bg-label-success';
          break;
        case 'Delay':
          categoryColors[category.name] = 'bg-label-warning';
          break;
        case 'Standby':
          categoryColors[category.name] = 'bg-label-info';
          break;
        case 'Service':
          categoryColors[category.name] = 'bg-label-primary';
          break;
        case 'Breakdown':
          categoryColors[category.name] = 'bg-label-danger';
          break;
        default:
          categoryColors[category.name] = 'bg-label-secondary';
      }
    });

    // Populate category dropdowns
    populateCategoryDropdowns();

    return categories;
  } catch (error) {
    console.error('Error loading categories:', error);
    showToast('Failed to load categories. Please refresh the page.', 'error');
    throw error;
  }
}

// Populate category dropdowns
function populateCategoryDropdowns() {
  // Clear existing options except the first one
  while (categorySelect.options.length > 1) {
    categorySelect.remove(1);
  }

  while (editCategorySelect.options.length > 1) {
    editCategorySelect.remove(1);
  }

  // Add new options
  categories.forEach(category => {
    // For create form
    const createOption = document.createElement('option');
    createOption.value = category.id;
    createOption.textContent = category.name;
    categorySelect.appendChild(createOption);

    // For edit form
    const editOption = document.createElement('option');
    editOption.value = category.id;
    editOption.textContent = category.name;
    editCategorySelect.appendChild(editOption);
  });
}

// Function to load subcategories
async function loadSubcategories() {
  showLoading();

  try {
    const response = await fetch('/api/UsageSubcategories');

    if (!response.ok) {
      throw new Error('Failed to fetch data');
    }

    subcategories = await response.json();

    if (subcategories.length === 0) {
      showNoData();
      return;
    }

    // Sort subcategories by category and name
    subcategories.sort((a, b) => {
      // First by category
      if (a.category !== b.category) {
        return a.category - b.category; // Category is enum (numeric) in DTO
      }
      // Then by name
      return a.name.localeCompare(b.name);
    });

    // Render the table
    renderSubcategoryTable();
    showTable();
  } catch (error) {
    console.error('Error loading subcategories:', error);
    showError();
  }
}

// Render the subcategories table
function renderSubcategoryTable() {
  subcategoryTableBody.innerHTML = '';

  subcategories.forEach(subcategory => {
    const row = document.createElement('tr');

    // Determine the category name
    let categoryName;

    // Check if category is already a string (from API)
    if (typeof subcategory.category === 'string') {
      categoryName = subcategory.category;
    }
    // Check if we have a categoryName property
    else if (subcategory.categoryName) {
      categoryName = subcategory.categoryName;
    }
    // Use the categoryMap to look up the name from the numeric ID
    else {
      categoryName = categoryMap[subcategory.category] || 'Unknown';
    }

    // Log for debugging
    console.log(
      `Subcategory ID: ${subcategory.id}, Name: ${subcategory.name}, Category: ${subcategory.category}, CategoryName: ${categoryName}`
    );

    // Get badge color for this category (default to secondary if not found)
    const badgeClass = categoryColors[categoryName] || 'bg-label-secondary';

    // Populate row
    row.innerHTML = `
      <td>${subcategory.id}</td>
      <td>${subcategory.name}</td>
      <td><span class="badge ${badgeClass}">${categoryName}</span></td>
      <td>
        <div class="dropdown">
          <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
            <i class="bx bx-dots-vertical-rounded"></i>
          </button>
          <div class="dropdown-menu">
            <a class="dropdown-item edit-subcategory" href="javascript:void(0);" data-id="${subcategory.id}">
              <i class="bx bx-edit-alt me-1"></i> Edit
            </a>
            <a class="dropdown-item delete-subcategory" href="javascript:void(0);" data-id="${subcategory.id}">
              <i class="bx bx-trash me-1"></i> Delete
            </a>
          </div>
        </div>
      </td>
    `;

    subcategoryTableBody.appendChild(row);
  });

  // Add event listeners to the newly created buttons
  document.querySelectorAll('.edit-subcategory').forEach(btn => {
    btn.addEventListener('click', handleEditClick);
  });

  document.querySelectorAll('.delete-subcategory').forEach(btn => {
    btn.addEventListener('click', handleDeleteClick);
  });
}

// Initialize event listeners
function initializeEventListeners() {
  // Create subcategory
  saveSubcategoryBtn.addEventListener('click', handleCreateSubcategory);

  // Update subcategory
  updateSubcategoryBtn.addEventListener('click', handleUpdateSubcategory);

  // Delete subcategory
  confirmDeleteBtn.addEventListener('click', handleDeleteSubcategory);

  // Reset form on modal close
  createSubcategoryModal.addEventListener('hidden.bs.modal', () => {
    createSubcategoryForm.reset();
  });
}

// Event handler for the edit button
function handleEditClick(e) {
  const subcategoryId = parseInt(e.currentTarget.dataset.id);
  const subcategory = subcategories.find(s => s.id === subcategoryId);

  if (subcategory) {
    // Populate the edit form
    document.getElementById('editSubcategoryId').value = subcategory.id;
    document.getElementById('editSubcategoryName').value = subcategory.name;

    // Handle potential different formats of category data
    let categoryId;

    // If category is a string (name), find the corresponding ID
    if (typeof subcategory.category === 'string') {
      // Find the category ID by name
      for (const category of categories) {
        if (category.name === subcategory.category) {
          categoryId = category.id;
          break;
        }
      }
    }
    // If we have a categoryName property and need to find its ID
    else if (subcategory.categoryName) {
      // Find the category ID by name
      for (const category of categories) {
        if (category.name === subcategory.categoryName) {
          categoryId = category.id;
          break;
        }
      }
    }
    // If we have a numeric category ID already
    else {
      categoryId = subcategory.category;
    }

    console.log(`Setting category select to ID: ${categoryId} for subcategory: ${subcategory.name}`);

    if (categoryId !== undefined) {
      document.getElementById('editCategorySelect').value = categoryId;
    }

    // Show the modal
    const modal = new bootstrap.Modal(editSubcategoryModal);
    modal.show();
  }
}

// Event handler for the delete button
function handleDeleteClick(e) {
  const subcategoryId = parseInt(e.currentTarget.dataset.id);
  deleteSubcategoryId.value = subcategoryId;

  // Show the modal
  const modal = new bootstrap.Modal(deleteSubcategoryModal);
  modal.show();
}

// Create a new subcategory
async function handleCreateSubcategory() {
  // Validate form
  if (!createSubcategoryForm.checkValidity()) {
    createSubcategoryForm.reportValidity();
    return;
  }

  // Disable the button during operation
  saveSubcategoryBtn.disabled = true;
  saveSubcategoryBtn.innerHTML =
    '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...';

  try {
    const formData = new FormData(createSubcategoryForm);

    const subcategoryData = {
      name: formData.get('name'),
      category: parseInt(formData.get('category'))
    };

    const response = await fetch('/api/UsageSubcategories', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(subcategoryData)
    });

    if (!response.ok) {
      const errorText = await response.text();
      let errorMessage = 'Failed to create subcategory';

      try {
        const errorData = JSON.parse(errorText);
        if (errorData.message) {
          errorMessage = errorData.message;
        }
      } catch (e) {
        // If can't parse as JSON, use the error text directly if it exists
        if (errorText) errorMessage = errorText;
      }

      throw new Error(errorMessage);
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(createSubcategoryModal);
    modal.hide();

    // Reset form
    createSubcategoryForm.reset();

    // Reload the data
    await loadSubcategories();

    // Show success message
    showToast('Subcategory created successfully!', 'success');
  } catch (error) {
    console.error('Error creating subcategory:', error);
    showToast(error.message || 'Failed to create subcategory', 'error');
  } finally {
    // Always re-enable the button
    saveSubcategoryBtn.disabled = false;
    saveSubcategoryBtn.innerHTML = 'Save';
  }
}

// Update an existing subcategory
async function handleUpdateSubcategory() {
  // Validate form
  if (!editSubcategoryForm.checkValidity()) {
    editSubcategoryForm.reportValidity();
    return;
  }

  // Disable the button during operation
  updateSubcategoryBtn.disabled = true;
  updateSubcategoryBtn.innerHTML =
    '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Updating...';

  try {
    const formData = new FormData(editSubcategoryForm);
    const subcategoryId = parseInt(formData.get('id'));

    const subcategoryData = {
      name: formData.get('name'),
      category: parseInt(formData.get('category'))
    };

    const response = await fetch(`/api/UsageSubcategories/${subcategoryId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(subcategoryData)
    });

    if (!response.ok) {
      const errorText = await response.text();
      let errorMessage = 'Failed to update subcategory';

      try {
        const errorData = JSON.parse(errorText);
        if (errorData.message) {
          errorMessage = errorData.message;
        }
      } catch (e) {
        // If can't parse as JSON, use the error text directly if it exists
        if (errorText) errorMessage = errorText;
      }

      throw new Error(errorMessage);
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(editSubcategoryModal);
    modal.hide();

    // Reload the data
    await loadSubcategories();

    // Show success message
    showToast('Subcategory updated successfully!', 'success');
  } catch (error) {
    console.error('Error updating subcategory:', error);
    showToast(error.message || 'Failed to update subcategory', 'error');
  } finally {
    // Always re-enable the button
    updateSubcategoryBtn.disabled = false;
    updateSubcategoryBtn.innerHTML = 'Update';
  }
}

// Delete a subcategory
async function handleDeleteSubcategory() {
  // Disable the button during operation
  confirmDeleteBtn.disabled = true;
  confirmDeleteBtn.innerHTML =
    '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Deleting...';

  try {
    const subcategoryId = parseInt(deleteSubcategoryId.value);

    const response = await fetch(`/api/UsageSubcategories/${subcategoryId}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      const errorText = await response.text();
      let errorMessage = 'Failed to delete subcategory';

      try {
        const errorData = JSON.parse(errorText);
        if (errorData.message) {
          errorMessage = errorData.message;
        }
      } catch (e) {
        // If can't parse as JSON, use the error text directly if it exists
        if (errorText) errorMessage = errorText;
      }

      throw new Error(errorMessage);
    }

    // Close the modal
    const modal = bootstrap.Modal.getInstance(deleteSubcategoryModal);
    modal.hide();

    // Reload the data
    await loadSubcategories();

    // Show success message
    showToast('Subcategory deleted successfully!', 'success');
  } catch (error) {
    console.error('Error deleting subcategory:', error);
    showToast(error.message || 'Failed to delete subcategory', 'error');
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
  subcategoryTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  subcategoryTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  subcategoryTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  subcategoryTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
