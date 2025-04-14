// Global variables for data
let cranes = [];
let shiftDefinitions = [];

// DOM Elements
const startDateInput = document.getElementById('startDate');
const endDateInput = document.getElementById('endDate');
const shiftTableContainer = document.getElementById('shiftTableContainer');
const shiftTableBody = document.getElementById('shiftTableBody');
const submitButton = document.getElementById('submitButton');
const craneIdSelect = document.getElementById('craneId');
const shiftLabel = document.getElementById('shiftLabel');
const spacerDiv = document.getElementById('spacerDiv');

// Set min date to today
const today = new Date();
const formattedToday = today.toISOString().split('T')[0];
startDateInput.min = formattedToday;
endDateInput.min = formattedToday;

// Initially, don't set default date values
startDateInput.value = '';
endDateInput.value = '';

// Load data on page load
document.addEventListener('DOMContentLoaded', async function () {
  // Set user data from ViewData
  setUserData();

  await loadShiftDefinitions();
  await loadCranes();
});

// Set user data from ViewData
function setUserData() {
  // Get values from hidden inputs
  const userName = document.getElementById('userName')?.value || '';

  // Set values to form fields
  document.getElementById('createdBy').value = userName;
}

// Fetch shift definitions from API
async function loadShiftDefinitions() {
  try {
    const response = await fetch('/api/ShiftDefinitions');
    if (!response.ok) {
      throw new Error('Failed to load shift definitions');
    }

    shiftDefinitions = await response.json();

    // Sort shift definitions by start time
    shiftDefinitions.sort((a, b) => {
      const timeA = a.startTime.split(':').map(Number);
      const timeB = b.startTime.split(':').map(Number);
      return timeA[0] * 60 + timeA[1] - (timeB[0] * 60 + timeB[1]);
    });

    console.log('Loaded shift definitions:', shiftDefinitions);
  } catch (error) {
    console.error('Error loading shift definitions:', error);
    alert('Failed to load shift definitions. Please refresh the page or contact support.');
  }
}

// Fetch cranes from API
async function loadCranes() {
  try {
    const response = await fetch('/api/Cranes');
    if (!response.ok) {
      throw new Error('Failed to load cranes');
    }

    cranes = await response.json();
    populateCraneDropdown();
  } catch (error) {
    console.error('Error loading cranes:', error);
    alert('Failed to load crane data. Please refresh the page or contact support.');
  }
}

// Populate crane dropdown
function populateCraneDropdown() {
  // Clear existing options except the first one
  while (craneIdSelect.options.length > 1) {
    craneIdSelect.remove(1);
  }

  // Add crane options
  cranes.forEach(crane => {
    const option = document.createElement('option');
    option.value = crane.id;
    option.textContent = `${crane.code} (${crane.capacity} ton) - ${crane.status}`;

    // Disable options if crane is not available
    if (crane.status !== 'Available') {
      option.disabled = true;
    }

    craneIdSelect.appendChild(option);
  });
}

// Validation functions
function validateDates() {
  const startDate = new Date(startDateInput.value);
  const endDate = new Date(endDateInput.value);
  const startDateError = document.getElementById('startDateError');
  const endDateError = document.getElementById('endDateError');

  startDateError.textContent = '';
  endDateError.textContent = '';

  if (!startDateInput.value) {
    startDateError.textContent = 'Start date is required';
    return false;
  }

  if (!endDateInput.value) {
    endDateError.textContent = 'End date is required';
    return false;
  }

  if (startDate > endDate) {
    endDateError.textContent = 'End date cannot be before start date';
    return false;
  }

  return true;
}

// Function to generate shift table
function generateShiftTable() {
  // Check if both dates are selected
  const startDate = startDateInput.value;
  const endDate = endDateInput.value;

  if (!startDate || !endDate) {
    // Hide the shift table if dates are not complete
    shiftTableContainer.style.display = 'none';
    shiftLabel.style.display = 'none';
    spacerDiv.style.display = 'block'; // Show spacer when shift table is hidden
    return;
  }

  if (!validateDates()) {
    // Hide the shift table if dates are invalid
    shiftTableContainer.style.display = 'none';
    shiftLabel.style.display = 'none';
    spacerDiv.style.display = 'block'; // Show spacer when shift table is hidden
    return;
  }

  // Generate dates between start and end date
  const start = new Date(startDate);
  const end = new Date(endDate);
  const dateArray = [];

  // Loop through the dates
  let currentDate = new Date(start);
  while (currentDate <= end) {
    dateArray.push({
      date: new Date(currentDate).toISOString().split('T')[0],
      selectedShiftIds: []
    });
    currentDate.setDate(currentDate.getDate() + 1);
  }

  renderShiftTable(dateArray);
}

function renderShiftTable(shiftTable) {
  if (!shiftTable || shiftTable.length === 0 || !shiftDefinitions || shiftDefinitions.length === 0) {
    shiftTableContainer.style.display = 'none';
    shiftLabel.style.display = 'none';
    spacerDiv.style.display = 'block'; // Show spacer when shift table is hidden
    return;
  }

  // Get the table header
  const tableHeader = shiftTableContainer.querySelector('thead tr');

  // Clear existing headers except the first one (Date)
  while (tableHeader.children.length > 1) {
    tableHeader.removeChild(tableHeader.lastChild);
  }

  // Add column headers for each shift definition
  shiftDefinitions.forEach(shift => {
    const th = document.createElement('th');
    th.textContent = shift.name;
    tableHeader.appendChild(th);
  });

  // Clear existing table body
  shiftTableBody.innerHTML = '';

  // Add rows for each date
  shiftTable.forEach((dayShift, index) => {
    const dateObj = new Date(dayShift.date);
    const formattedDate = dateObj.toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });

    const row = document.createElement('tr');
    row.dataset.date = dayShift.date;

    // Add date cell
    const dateCell = document.createElement('td');
    dateCell.textContent = formattedDate;
    row.appendChild(dateCell);

    // Add checkboxes for each shift definition
    shiftDefinitions.forEach(shift => {
      const isSelected = dayShift.selectedShiftIds.includes(shift.id);

      const cell = document.createElement('td');
      cell.innerHTML = `
        <div class="form-check d-flex justify-content-center">
          <input type="checkbox" class="form-check-input shift-checkbox"
            id="shift-${shift.id}-${dayShift.date}"
            data-date="${dayShift.date}"
            data-shift-id="${shift.id}"
            ${isSelected ? 'checked' : ''} />
        </div>
      `;

      row.appendChild(cell);
    });

    shiftTableBody.appendChild(row);
  });

  // Show the table container
  shiftTableContainer.style.display = 'block';
  shiftLabel.style.display = 'block';
  spacerDiv.style.display = 'none'; // Hide spacer when shift table is visible

  // Add event listeners to check for conflicts
  const craneId = document.getElementById('craneId').value;
  if (craneId) {
    const shiftCheckboxes = document.querySelectorAll('.shift-checkbox');

    shiftCheckboxes.forEach(checkbox => {
      checkbox.addEventListener('change', function () {
        checkShiftConflict(this);
      });
    });
  }
}

// Function to check for shift conflicts with the API
async function checkShiftConflict(checkbox) {
  const craneId = document.getElementById('craneId').value;
  if (!craneId) return;

  const date = checkbox.dataset.date;
  const shiftId = parseInt(checkbox.dataset.shiftId);
  const isChecked = checkbox.checked;

  // Only check if the checkbox is being checked (not unchecked)
  if (isChecked) {
    try {
      // Check conflict with other maintenance schedules only
      // Maintenance is prioritized, so we don't need to check conflicts with bookings
      const maintenanceResponse = await fetch(
        `/api/MaintenanceSchedules/CheckShiftConflict?craneId=${craneId}&date=${date}&shiftDefinitionId=${shiftId}`
      );

      if (!maintenanceResponse.ok) {
        throw new Error('Failed to check maintenance conflict');
      }

      const hasMaintenanceConflict = await maintenanceResponse.json();

      if (hasMaintenanceConflict) {
        // Find shift name for better error message
        const shiftName = shiftDefinitions.find(s => s.id === shiftId)?.name || `Shift ${shiftId}`;
        alert(
          `There is already a maintenance schedule for this crane on ${new Date(date).toLocaleDateString()} during ${shiftName}. Please select a different shift or crane.`
        );
        checkbox.checked = false;
      }
    } catch (error) {
      console.error('Error checking conflict:', error);
    }
  }
}

// Auto-generate shift table when dates change
startDateInput.addEventListener('change', generateShiftTable);
endDateInput.addEventListener('change', generateShiftTable);

// Regenerate shift table when crane changes to check for conflicts
craneIdSelect.addEventListener('change', function () {
  if (startDateInput.value && endDateInput.value) {
    generateShiftTable();
  }
});

// Function to collect form data
function collectFormData() {
  // Basic form data
  const formData = {
    craneId: parseInt(document.getElementById('craneId').value),
    title: document.getElementById('title').value,
    startDate: new Date(document.getElementById('startDate').value).toISOString(),
    endDate: new Date(document.getElementById('endDate').value).toISOString(),
    description: document.getElementById('description').value,
    createdBy: document.getElementById('createdBy').value
  };

  // Collect shift selections
  formData.shiftSelections = [];
  const shiftRows = shiftTableBody.querySelectorAll('tr');

  shiftRows.forEach(row => {
    const dateStr = row.dataset.date;
    const checkboxes = row.querySelectorAll('.shift-checkbox:checked');

    if (checkboxes.length > 0) {
      const selectedShiftIds = Array.from(checkboxes).map(checkbox => parseInt(checkbox.dataset.shiftId));

      formData.shiftSelections.push({
        date: new Date(dateStr).toISOString(),
        selectedShiftIds: selectedShiftIds
      });
    }
  });

  return formData;
}

// Validate the form
function validateForm() {
  let isValid = true;
  const shiftTableError = document.getElementById('shiftTableError');

  // Clear previous errors
  shiftTableError.textContent = '';

  // Validate dates
  if (!validateDates()) {
    isValid = false;
  }

  // Validate title
  if (!document.getElementById('title').value.trim()) {
    isValid = false;
  }

  // Validate crane selection
  if (!document.getElementById('craneId').value) {
    isValid = false;
  }

  // Validate shift table has at least one selection
  if (shiftTableContainer.style.display === 'none') {
    shiftTableError.textContent = 'Please select start and end dates to generate shifts';
    isValid = false;
  } else {
    // Check if at least one shift is selected
    const selectedShifts = document.querySelectorAll('.shift-checkbox:checked');
    if (selectedShifts.length === 0) {
      shiftTableError.textContent = 'Please select at least one shift';
      isValid = false;
    }
  }

  // Validate description
  if (!document.getElementById('description').value.trim()) {
    isValid = false;
  }

  return isValid;
}

// Log form data for debugging
function logFormData(formData) {
  console.log('Submitting maintenance schedule with data:', JSON.stringify(formData, null, 2));
}

// Submit maintenance schedule
async function submitMaintenanceSchedule(formData) {
  // Log form data for debugging
  logFormData(formData);

  try {
    const response = await fetch('/api/MaintenanceSchedules', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(formData)
    });

    if (!response.ok) {
      // Handle error responses from the server
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to create maintenance schedule');
    }

    // Show success modal
    const successModal = new bootstrap.Modal(document.getElementById('successModal'));
    successModal.show();

    // Reset form after successful submission
    document.getElementById('craneMaintenanceForm').reset();
    shiftTableContainer.style.display = 'none';
  } catch (error) {
    console.error('Error submitting maintenance schedule:', error);

    // Show error modal
    const errorModal = new bootstrap.Modal(document.getElementById('errorModal'));
    document.getElementById('errorModalBody').textContent =
      error.message || 'An error occurred while submitting your maintenance schedule.';
    errorModal.show();
  }
}

// Submit button click handler
submitButton.addEventListener('click', function () {
  if (validateForm()) {
    const formData = collectFormData();
    submitMaintenanceSchedule(formData);
  } else {
    // Scroll to the first error message
    const firstError = document.querySelector('.text-danger:not(:empty)');
    if (firstError) {
      firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }
});
