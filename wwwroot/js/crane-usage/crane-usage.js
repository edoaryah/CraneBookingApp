// Global variables
let bookingId = null;
let bookingDetails = null;
let usageSummary = null;
let usageRecords = [];
let hoursChart = null;
let categorySubcategories = {};

// Initialize the page
document.addEventListener('DOMContentLoaded', function () {
  // Get booking ID from URL
  const pathParts = window.location.pathname.split('/');
  bookingId = pathParts[pathParts.length - 1];

  if (!bookingId || isNaN(parseInt(bookingId))) {
    showError('Invalid booking ID');
    return;
  }

  // Set up event listeners
  setupEventListeners();

  // Initialize the page with data
  initCraneUsagePage(parseInt(bookingId));
});

// Set up event listeners
function setupEventListeners() {
  // Toggle add record form
  document.getElementById('addRecordBtn').addEventListener('click', toggleAddRecordForm);
  document.getElementById('closeFormBtn').addEventListener('click', toggleAddRecordForm);
  document.getElementById('cancelAddBtn').addEventListener('click', toggleAddRecordForm);

  // Category change event for new record form
  document.getElementById('category').addEventListener('change', function () {
    loadSubcategories(this.value, 'subcategory');
  });

  // Category change event for edit record form
  document.getElementById('editCategory').addEventListener('change', function () {
    loadSubcategories(this.value, 'editSubcategory');
  });

  // Form submission for new record
  document.getElementById('usageRecordForm').addEventListener('submit', function (e) {
    e.preventDefault();
    saveNewRecord();
  });

  // Update record button
  document.getElementById('updateRecordBtn').addEventListener('click', updateRecord);
}

// Initialize the page with data
async function initCraneUsagePage(id) {
  try {
    // Load booking details
    bookingDetails = await fetchBookingDetails(id);
    displayBookingInfo(bookingDetails);

    // Set default date range for the booking
    setDateRangeForBooking(bookingDetails);

    // Load usage summary
    usageSummary = await fetchUsageSummary(id);
    usageRecords = usageSummary.usageRecords || [];

    // Display data
    displayUsageSummary(usageSummary);
    displayUsageRecords(usageRecords);
    initializeCharts(usageSummary);

    // Hide loading indicator and show content
    document.getElementById('loadingIndicator').style.display = 'none';
    document.getElementById('craneUsageContent').style.display = 'block';
  } catch (error) {
    console.error('Error initializing page:', error);
    showError('Failed to load data: ' + error.message);
  }
}

// Toggle add record form visibility
function toggleAddRecordForm() {
  const form = document.getElementById('addRecordForm');
  if (form.style.display === 'none' || form.style.display === '') {
    form.style.display = 'block';
    // Reset form
    document.getElementById('usageRecordForm').reset();
    // Set default date to today (within booking range)
    setDefaultDate();
  } else {
    form.style.display = 'none';
  }
}

// Set default date for new record (today or within booking range)
function setDefaultDate() {
  const today = new Date();
  const startDate = new Date(bookingDetails.startDate);
  const endDate = new Date(bookingDetails.endDate);

  let defaultDate = today;
  if (today < startDate) {
    defaultDate = startDate;
  } else if (today > endDate) {
    defaultDate = endDate;
  }

  document.getElementById('recordDate').value = formatDateForInput(defaultDate);
  document.getElementById('recordDate').min = formatDateForInput(startDate);
  document.getElementById('recordDate').max = formatDateForInput(endDate);
}

// Set date range input constraints based on booking
function setDateRangeForBooking(booking) {
  const startDate = new Date(booking.startDate);
  const endDate = new Date(booking.endDate);

  document.getElementById('recordDate').min = formatDateForInput(startDate);
  document.getElementById('recordDate').max = formatDateForInput(endDate);
}

// Format date for input (YYYY-MM-DD)
function formatDateForInput(date) {
  return date.toISOString().split('T')[0];
}

// Display booking information
function displayBookingInfo(booking) {
  // Set page header
  document.getElementById('bookingNumber').textContent = `Booking #${booking.bookingNumber}`;

  // Set booking details
  document.getElementById('bookingNumberValue').textContent = booking.bookingNumber;
  document.getElementById('nameValue').textContent = booking.name;
  document.getElementById('departmentValue').textContent = booking.department;
  document.getElementById('craneValue').textContent = booking.craneCode;

  // Format date range
  const startDate = new Date(booking.startDate).toLocaleDateString('id-ID');
  const endDate = new Date(booking.endDate).toLocaleDateString('id-ID');
  document.getElementById('dateRangeValue').textContent = `${startDate} - ${endDate}`;

  // Set other details
  document.getElementById('locationValue').textContent = booking.location || '-';
  document.getElementById('descriptionValue').textContent = booking.description || '-';
}

// Display usage summary statistics
function displayUsageSummary(summary) {
  // If no summary data available, use default values
  if (!summary) {
    return;
  }

  // Set availability percentage
  const availabilityPercentage = summary.availabilityPercentage || 0;
  document.getElementById('availabilityValue').textContent = `${availabilityPercentage.toFixed(2)}%`;
  document.getElementById('availabilityProgress').style.width = `${availabilityPercentage}%`;
  document.getElementById('availabilityProgress').setAttribute('aria-valuenow', availabilityPercentage);

  // Set utilisation percentage
  const utilisationPercentage = summary.utilisationPercentage || 0;
  document.getElementById('utilisationValue').textContent = `${utilisationPercentage.toFixed(2)}%`;
  document.getElementById('utilisationProgress').style.width = `${utilisationPercentage}%`;
  document.getElementById('utilisationProgress').setAttribute('aria-valuenow', utilisationPercentage);

  // Set hours by category
  document.getElementById('operatingHours').textContent = formatTimeSpan(summary.totalOperatingTime);
  document.getElementById('delayHours').textContent = formatTimeSpan(summary.totalDelayTime);
  document.getElementById('standbyHours').textContent = formatTimeSpan(summary.totalStandbyTime);
}

// Format TimeSpan to HH:MM
function formatTimeSpan(timeSpan) {
  if (!timeSpan) return '00:00';

  // Parse the timespan format "00:00:00"
  const parts = timeSpan.split(':');
  let hours = parseInt(parts[0]);
  const minutes = parseInt(parts[1]);

  // Format as HH:MM
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
}

// Display usage records in table
function displayUsageRecords(records) {
  const tableBody = document.getElementById('usageRecordsBody');
  const table = document.getElementById('usageRecordsTable');
  const noRecordsMessage = document.getElementById('noRecordsMessage');

  tableBody.innerHTML = '';

  if (!records || records.length === 0) {
    table.style.display = 'none';
    noRecordsMessage.style.display = 'block';
    return;
  }

  table.style.display = 'table';
  noRecordsMessage.style.display = 'none';

  // Sort records by date (newest first)
  records.sort((a, b) => new Date(b.date) - new Date(a.date));

  records.forEach(record => {
    const row = document.createElement('tr');
    row.classList.add('usage-record-row');

    // Format date
    const recordDate = new Date(record.date).toLocaleDateString('id-ID');

    // Get category display name
    const categoryName = record.categoryName;
    const categoryLower = categoryName.toLowerCase();

    // Create category badge
    const categoryBadge = `<span class="badge ${categoryLower}">${categoryName}</span>`;

    row.innerHTML = `
      <td>${recordDate}</td>
      <td>${categoryBadge}</td>
      <td>${record.subcategoryName}</td>
      <td>${record.durationFormatted}</td>
      <td class="text-end">
        <div class="dropdown">
          <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
            <i class="bx bx-dots-vertical-rounded"></i>
          </button>
          <div class="dropdown-menu">
            <a class="dropdown-item edit-record" href="javascript:void(0);" data-record-id="${record.id}">
              <i class="bx bx-edit-alt me-1"></i> Edit
            </a>
            <a class="dropdown-item delete-record" href="javascript:void(0);" data-record-id="${record.id}">
              <i class="bx bx-trash me-1"></i> Delete
            </a>
          </div>
        </div>
      </td>
    `;

    tableBody.appendChild(row);

    // Add event listeners for edit and delete
    row.querySelector('.edit-record').addEventListener('click', function () {
      openEditModal(record);
    });

    row.querySelector('.delete-record').addEventListener('click', function () {
      openDeleteModal(record.id);
    });
  });
}

// Initialize charts
function initializeCharts(summary) {
  const ctx = document.getElementById('hoursChart').getContext('2d');

  if (hoursChart) {
    hoursChart.destroy();
  }

  // Get hours for each category
  const operatingHours = timeSpanToHours(summary.totalOperatingTime);
  const delayHours = timeSpanToHours(summary.totalDelayTime);
  const standbyHours = timeSpanToHours(summary.totalStandbyTime);
  const serviceHours = timeSpanToHours(summary.totalServiceTime);
  const breakdownHours = timeSpanToHours(summary.totalBreakdownTime);

  hoursChart = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: ['Operating', 'Delay', 'Standby', 'Service', 'Breakdown'],
      datasets: [
        {
          label: 'Hours',
          data: [operatingHours, delayHours, standbyHours, serviceHours, breakdownHours],
          backgroundColor: [
            '#696cff', // Operating - Primary
            '#ffab00', // Delay - Warning
            '#03c3ec', // Standby - Info
            '#71dd37', // Service - Success
            '#ff3e1d' // Breakdown - Danger
          ],
          borderWidth: 0
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: 'Hours'
          }
        }
      },
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              const value = context.raw;
              return `${value.toFixed(2)} hours`;
            }
          }
        }
      }
    }
  });
}

// Convert timespan to hours
function timeSpanToHours(timeSpan) {
  if (!timeSpan) return 0;

  // Parse the timespan format "00:00:00"
  const parts = timeSpan.split(':');
  const hours = parseInt(parts[0]);
  const minutes = parseInt(parts[1]);

  // Convert to decimal hours
  return hours + minutes / 60;
}

// Fetch booking details from API
async function fetchBookingDetails(id) {
  try {
    const response = await fetch(`/api/Bookings/${id}`, {
      headers: {
        Authorization: `Bearer ${getToken()}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error fetching booking details:', error);
    throw error;
  }
}

// Fetch usage summary from API
async function fetchUsageSummary(id) {
  try {
    const response = await fetch(`/api/CraneUsageRecords/Summary/${id}`, {
      headers: {
        Authorization: `Bearer ${getToken()}`
      }
    });

    if (!response.ok) {
      if (response.status === 404) {
        // No usage records yet, return empty summary
        return {
          bookingId: id,
          bookingNumber: bookingDetails.bookingNumber,
          date: bookingDetails.startDate,
          usageRecords: [],
          totalOperatingTime: '00:00:00',
          totalDelayTime: '00:00:00',
          totalStandbyTime: '00:00:00',
          totalServiceTime: '00:00:00',
          totalBreakdownTime: '00:00:00',
          totalAvailableTime: '00:00:00',
          totalUnavailableTime: '00:00:00',
          totalUsageTime: '00:00:00',
          availabilityPercentage: 0,
          utilisationPercentage: 0
        };
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error fetching usage summary:', error);
    throw error;
  }
}

// Load subcategories for a category
async function loadSubcategories(categoryValue, targetSelectId) {
  const categoryId = parseInt(categoryValue);
  const targetSelect = document.getElementById(targetSelectId);

  // Clear current options
  targetSelect.innerHTML = '<option value="">Loading subcategories...</option>';

  try {
    // Check if we already have the subcategories cached
    if (categorySubcategories[categoryId]) {
      populateSubcategorySelect(targetSelect, categorySubcategories[categoryId]);
      return;
    }

    // Fetch subcategories from API
    const response = await fetch(`/api/CraneUsageRecords/Subcategories/${categoryId}`, {
      headers: {
        Authorization: `Bearer ${getToken()}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const subcategories = await response.json();

    // Cache the subcategories
    categorySubcategories[categoryId] = subcategories;

    // Populate the select
    populateSubcategorySelect(targetSelect, subcategories);
  } catch (error) {
    console.error('Error loading subcategories:', error);
    targetSelect.innerHTML = '<option value="">Error loading subcategories</option>';
  }
}

// Populate subcategory select with options
function populateSubcategorySelect(selectElement, subcategories) {
  selectElement.innerHTML = '';

  if (!subcategories || subcategories.length === 0) {
    selectElement.innerHTML = '<option value="">No subcategories available</option>';
    return;
  }

  // Add default option
  const defaultOption = document.createElement('option');
  defaultOption.value = '';
  defaultOption.textContent = 'Select a subcategory';
  selectElement.appendChild(defaultOption);

  // Add subcategories
  subcategories.forEach(subcategory => {
    const option = document.createElement('option');
    option.value = subcategory.id;
    option.textContent = subcategory.name;
    selectElement.appendChild(option);
  });
}

// Save new usage record
async function saveNewRecord() {
  const form = document.getElementById('usageRecordForm');

  // Validate form
  if (!form.checkValidity()) {
    form.classList.add('was-validated');
    return;
  }

  // Get form values
  const date = document.getElementById('recordDate').value;
  const category = parseInt(document.getElementById('category').value);
  const subcategoryId = parseInt(document.getElementById('subcategory').value);
  const duration = document.getElementById('duration').value;

  // Create record object
  const recordData = {
    bookingId: bookingId,
    date: date,
    category: category,
    subcategoryId: subcategoryId,
    duration: duration
  };

  try {
    // Submit to API
    const response = await fetch('/api/CraneUsageRecords', {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(recordData)
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    // Get the new record
    const newRecord = await response.json();

    // Hide the form
    toggleAddRecordForm();

    // Reload page data
    initCraneUsagePage(bookingId);

    // Show success message
    showToast('Record added successfully');
  } catch (error) {
    console.error('Error saving record:', error);
    showToast('Error saving record: ' + error.message, 'error');
  }
}

// Open edit modal for a record
function openEditModal(record) {
  // Set record ID for update
  document.getElementById('editRecordId').value = record.id;

  // Set current values
  document.getElementById('editCategory').value = record.category;

  // Load subcategories for this category
  loadSubcategories(record.category, 'editSubcategory').then(() => {
    // Set subcategory after subcategories are loaded
    document.getElementById('editSubcategory').value = record.subcategoryId;
  });

  // Set duration
  document.getElementById('editDuration').value = record.durationFormatted;

  // Show modal
  const modal = new bootstrap.Modal(document.getElementById('editRecordModal'));
  modal.show();
}

// Update usage record
async function updateRecord() {
  const recordId = document.getElementById('editRecordId').value;
  const category = parseInt(document.getElementById('editCategory').value);
  const subcategoryId = parseInt(document.getElementById('editSubcategory').value);
  const duration = document.getElementById('editDuration').value;

  // Validate inputs
  if (!recordId || !category || !subcategoryId || !duration) {
    showToast('Please fill all fields', 'error');
    return;
  }

  // Create update data
  const updateData = {
    category: category,
    subcategoryId: subcategoryId,
    duration: duration
  };

  try {
    // Submit to API
    const response = await fetch(`/api/CraneUsageRecords/${recordId}`, {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updateData)
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    // Hide modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('editRecordModal'));
    modal.hide();

    // Reload page data
    initCraneUsagePage(bookingId);

    // Show success message
    showToast('Record updated successfully');
  } catch (error) {
    console.error('Error updating record:', error);
    showToast('Error updating record: ' + error.message, 'error');
  }
}

// Open delete confirmation modal
function openDeleteModal(recordId) {
  const confirmBtn = document.getElementById('confirmDeleteBtn');
  confirmBtn.setAttribute('data-record-id', recordId);

  // Show modal
  const modal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
  modal.show();

  // Set up event handler once
  confirmBtn.onclick = function () {
    const id = this.getAttribute('data-record-id');
    deleteRecord(id);

    // Hide modal
    const deleteModal = bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal'));
    deleteModal.hide();
  };
}

// Delete usage record
async function deleteRecord(recordId) {
  try {
    // Submit to API
    const response = await fetch(`/api/CraneUsageRecords/${recordId}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${getToken()}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Reload page data
    initCraneUsagePage(bookingId);

    // Show success message
    showToast('Record deleted successfully');
  } catch (error) {
    console.error('Error deleting record:', error);
    showToast('Error deleting record: ' + error.message, 'error');
  }
}

// Show error message
function showError(message) {
  const errorMsg = document.getElementById('errorMessage');
  errorMsg.textContent = message;
  errorMsg.style.display = 'block';
  document.getElementById('loadingIndicator').style.display = 'none';
}

// Show toast notification
function showToast(message, type = 'success') {
  // If using a toast library, you would show toast here
  // For now, let's just use alert
  const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';

  const toast = document.createElement('div');
  toast.className = `toast align-items-center ${alertClass} border-0 position-fixed bottom-0 end-0 m-3`;
  toast.setAttribute('role', 'alert');
  toast.setAttribute('aria-live', 'assertive');
  toast.setAttribute('aria-atomic', 'true');

  toast.innerHTML = `
    <div class="d-flex">
      <div class="toast-body">
        ${message}
      </div>
      <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
  `;

  document.body.appendChild(toast);

  // Initialize toast
  const bsToast = new bootstrap.Toast(toast, { autohide: true, delay: 5000 });
  bsToast.show();

  // Remove from DOM after hidden
  toast.addEventListener('hidden.bs.toast', function () {
    document.body.removeChild(toast);
  });
}

// Get authentication token from localStorage
function getToken() {
  return localStorage.getItem('token');
}
