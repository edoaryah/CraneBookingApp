// Global variables for data
let cranes = [];
let hazards = [];

// DOM Elements
const startDateInput = document.getElementById('startDate');
const endDateInput = document.getElementById('endDate');
const shiftTableContainer = document.getElementById('shiftTableContainer');
const submitButton = document.getElementById('submitButton');
const craneIdSelect = document.getElementById('craneId');
const hazardsContainer = document.getElementById('hazardsContainer');
const addItemBtn = document.getElementById('addItemBtn');

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
  await loadCranes();
  await loadHazards();
  initLiftedItemsTable();

  // Add event listener for the add item button
  addItemBtn.addEventListener('click', addLiftedItemRow);
});

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

// Fetch hazards from API
async function loadHazards() {
  try {
    const response = await fetch('/api/Hazards');
    if (!response.ok) {
      throw new Error('Failed to load hazards');
    }

    hazards = await response.json();
    populateHazardsCheckboxes();
  } catch (error) {
    console.error('Error loading hazards:', error);
    alert('Failed to load hazard data. Please refresh the page or contact support.');
  }
}

// Populate hazards checkboxes
function populateHazardsCheckboxes() {
  hazardsContainer.innerHTML = '';

  hazards.forEach(hazard => {
    const col = document.createElement('div');
    col.className = 'col-md-4 mb-2';

    col.innerHTML = `
            <div class="form-check">
              <input class="form-check-input hazard-checkbox" type="checkbox"
                     name="hazard-${hazard.id}"
                     id="hazard-${hazard.id}"
                     value="${hazard.id}" />
              <label class="form-check-label" for="hazard-${hazard.id}">
                ${hazard.name}
              </label>
            </div>
          `;

    hazardsContainer.appendChild(col);
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
    return;
  }

  if (!validateDates()) {
    // Hide the shift table if dates are invalid
    shiftTableContainer.style.display = 'none';
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
      isDayShift: false,
      isNightShift: false
    });
    currentDate.setDate(currentDate.getDate() + 1);
  }

  renderShiftTable(dateArray);
}

function renderShiftTable(shiftTable) {
  if (!shiftTable || shiftTable.length === 0) {
    shiftTableContainer.innerHTML = '<p>No shifts to display</p>';
    shiftTableContainer.style.display = 'none';
    return;
  }

  let tableHtml = `
          <h6 class="mb-3">Shift Selection</h6>
          <table class="shift-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Day Shift (7am-7pm)</th>
                <th>Night Shift (7pm-7am)</th>
              </tr>
            </thead>
            <tbody>
        `;

  shiftTable.forEach((shift, index) => {
    const dateObj = new Date(shift.date);
    const formattedDate = dateObj.toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });

    tableHtml += `
            <tr data-date="${shift.date}">
              <td>${formattedDate}</td>
              <td>
                <input type="checkbox" class="shift-checkbox day-shift"
                  id="day-${shift.date}"
                  data-date="${shift.date}"
                  ${shift.isDayShift ? 'checked' : ''} />
              </td>
              <td>
                <input type="checkbox" class="shift-checkbox night-shift"
                  id="night-${shift.date}"
                  data-date="${shift.date}"
                  ${shift.isNightShift ? 'checked' : ''} />
              </td>
            </tr>
          `;
  });

  tableHtml += `
            </tbody>
          </table>
        `;

  shiftTableContainer.innerHTML = tableHtml;
  shiftTableContainer.style.display = 'block';

  // Add event listeners to check for conflicts
  const craneId = document.getElementById('craneId').value;
  if (craneId) {
    const dayShiftCheckboxes = document.querySelectorAll('.day-shift');
    const nightShiftCheckboxes = document.querySelectorAll('.night-shift');

    dayShiftCheckboxes.forEach(checkbox => {
      checkbox.addEventListener('change', function () {
        checkShiftConflict(this, true, false);
      });
    });

    nightShiftCheckboxes.forEach(checkbox => {
      checkbox.addEventListener('change', function () {
        checkShiftConflict(this, false, true);
      });
    });
  }
}

// Function to check for shift conflicts with the API
async function checkShiftConflict(checkbox, isDayShift, isNightShift) {
  const craneId = document.getElementById('craneId').value;
  if (!craneId) return;

  const date = checkbox.dataset.date;
  const isChecked = checkbox.checked;

  // Only check if the checkbox is being checked (not unchecked)
  if (isChecked) {
    try {
      const response = await fetch(`/api/Bookings/CheckConflict?craneId=${craneId}&date=${date}&isDayShift=${isDayShift}&isNightShift=${isNightShift}`);

      if (!response.ok) {
        throw new Error('Failed to check conflict');
      }

      const hasConflict = await response.json();

      if (hasConflict) {
        alert(`There is already a booking for this crane on ${new Date(date).toLocaleDateString()} during the selected shift. Please select a different shift or crane.`);
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

// Initialize the lifted items table with one row
function initLiftedItemsTable() {
  const tbody = document.getElementById('liftedItemsBody');
  tbody.innerHTML = ''; // Clear existing rows
  addLiftedItemRow();
}

// Add a new row to the lifted items table
function addLiftedItemRow() {
  const tbody = document.getElementById('liftedItemsBody');
  const rowIndex = tbody.rows.length;

  const row = document.createElement('tr');
  row.innerHTML = `
          <td>
            <input type="text" class="form-control item-name" required />
          </td>
          <td>
            <input type="number" class="form-control item-height"
                   min="0.01" step="0.01" required />
          </td>
          <td>
            <input type="number" class="form-control item-weight"
                   min="0.01" step="0.01" required />
          </td>
          <td>
            <input type="number" class="form-control item-quantity"
                   min="1" step="1" value="1" required />
          </td>
          <td>
            <button type="button" class="btn btn-outline-danger btn-sm remove-item-btn">
              <i class="bx bx-trash"></i>
            </button>
          </td>
        `;

  // Add event listener to remove button
  const removeBtn = row.querySelector('.remove-item-btn');
  removeBtn.addEventListener('click', function () {
    // Don't allow removing if it's the only row
    if (tbody.rows.length > 1) {
      row.remove();
    }
  });

  tbody.appendChild(row);
}

// Function to collect form data
function collectFormData() {
  // Basic form data
  const formData = {
    name: document.getElementById('name').value,
    department: document.getElementById('department').value,
    craneId: parseInt(document.getElementById('craneId').value),
    startDate: new Date(document.getElementById('startDate').value).toISOString(),
    endDate: new Date(document.getElementById('endDate').value).toISOString(),
    location: document.getElementById('location').value,
    projectSupervisor: document.getElementById('projectSupervisor').value,
    costCode: document.getElementById('costCode').value,
    phoneNumber: document.getElementById('phoneNumber').value,
    description: document.getElementById('description').value,
    customHazard: document.getElementById('customHazard').value || null
  };

  // Collect shift selections
  formData.shiftSelections = [];
  const shiftRows = shiftTableContainer.querySelectorAll('tbody tr');

  shiftRows.forEach(row => {
    const dateStr = row.dataset.date;
    const dayShift = row.querySelector('.day-shift').checked;
    const nightShift = row.querySelector('.night-shift').checked;

    if (dayShift || nightShift) {
      formData.shiftSelections.push({
        date: new Date(dateStr).toISOString(),
        isDayShift: dayShift,
        isNightShift: nightShift
      });
    }
  });

  // Collect items
  formData.items = [];
  const itemRows = document.querySelectorAll('#liftedItemsBody tr');

  itemRows.forEach(row => {
    const itemName = row.querySelector('.item-name').value;
    const height = parseFloat(row.querySelector('.item-height').value);
    const weight = parseFloat(row.querySelector('.item-weight').value);
    const quantity = parseInt(row.querySelector('.item-quantity').value);

    if (itemName && !isNaN(height) && !isNaN(weight) && !isNaN(quantity)) {
      formData.items.push({
        itemName,
        height,
        weight,
        quantity
      });
    }
  });

  // Collect hazard IDs
  formData.hazardIds = [];
  const hazardCheckboxes = document.querySelectorAll('.hazard-checkbox:checked');

  hazardCheckboxes.forEach(checkbox => {
    formData.hazardIds.push(parseInt(checkbox.value));
  });

  return formData;
}

// Validate the form
function validateForm() {
  let isValid = true;
  const shiftTableError = document.getElementById('shiftTableError');
  const hazardsError = document.getElementById('hazardsError');
  const liftedItemsError = document.getElementById('liftedItemsError');

  // Clear previous errors
  shiftTableError.textContent = '';
  hazardsError.textContent = '';
  liftedItemsError.textContent = '';

  // Validate dates
  if (!validateDates()) {
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

  // Validate items (at least one complete item)
  const items = document.querySelectorAll('#liftedItemsBody tr');
  let hasValidItem = false;

  for (let i = 0; i < items.length; i++) {
    const item = items[i];
    const nameInput = item.querySelector('.item-name');
    const heightInput = item.querySelector('.item-height');
    const weightInput = item.querySelector('.item-weight');
    const quantityInput = item.querySelector('.item-quantity');

    if (
      nameInput.value &&
      heightInput.value &&
      weightInput.value &&
      quantityInput.value
    ) {
      hasValidItem = true;
      break;
    }
  }

  if (!hasValidItem) {
    liftedItemsError.textContent = 'Please provide at least one item to be lifted';
    isValid = false;
  }

  // Validate hazards
  const selectedHazards = document.querySelectorAll('.hazard-checkbox:checked');
  if (selectedHazards.length === 0) {
    hazardsError.textContent = 'Please select at least one potential hazard';
    isValid = false;
  }

  // Validate terms agreement
  if (!document.getElementById('termsAgreement').checked) {
    isValid = false;
  }

  return isValid;
}

// Submit booking
async function submitBooking(formData) {
  try {
    const response = await fetch('/api/Bookings', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(formData)
    });

    if (!response.ok) {
      // Handle error responses from the server
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to create booking');
    }

    // Show success modal
    const successModal = new bootstrap.Modal(document.getElementById('successModal'));
    successModal.show();

    // Reset form after successful submission
    document.getElementById('craneBookingForm').reset();
    shiftTableContainer.style.display = 'none';
    initLiftedItemsTable();

  } catch (error) {
    console.error('Error submitting booking:', error);

    // Show error modal
    const errorModal = new bootstrap.Modal(document.getElementById('errorModal'));
    document.getElementById('errorModalBody').textContent = error.message || 'An error occurred while submitting your booking.';
    errorModal.show();
  }
}

// Submit button click handler
submitButton.addEventListener('click', function () {
  if (validateForm()) {
    const formData = collectFormData();
    submitBooking(formData);
  }
});
