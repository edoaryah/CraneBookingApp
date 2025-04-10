// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const errorMessage = document.getElementById('errorMessage');
const bookingDetailsForm = document.getElementById('bookingDetailsForm');
const bookingNumber = document.getElementById('bookingNumber');
const shiftTableContainer = document.getElementById('shiftTableContainer').querySelector('.table-responsive');
const liftedItemsBody = document.getElementById('liftedItemsBody');
const hazardsContainer = document.getElementById('hazardsContainer');
const customHazardContainer = document.getElementById('customHazardContainer');
const customHazardText = document.getElementById('customHazardText');

// Load booking details
async function loadBookingDetails(id) {
  showLoading();

  try {
    // Fetch booking details only from the single API endpoint
    const bookingResponse = await fetch(`/api/Bookings/${id}`);

    if (!bookingResponse.ok) {
      throw new Error('Failed to fetch booking data');
    }

    // Process the response
    const bookingData = await bookingResponse.json();

    // Populate the form
    populateBookingDetails(bookingData);

    // Show the form
    showForm();
  } catch (error) {
    console.error('Error loading booking details:', error);
    showError();
  }
}

// Populate booking details in the form
function populateBookingDetails(bookingData) {
  if (!bookingData) return;

  // Set booking number
  bookingNumber.textContent = bookingData.bookingNumber;

  // Basic information
  document.getElementById('name').value = bookingData.name || '';
  document.getElementById('department').value = bookingData.department || '';
  document.getElementById('projectSupervisor').value = bookingData.projectSupervisor || '';
  document.getElementById('phoneNumber').value = bookingData.phoneNumber || '';
  document.getElementById('costCode').value = bookingData.costCode || '';

  // Format dates for input fields (YYYY-MM-DD)
  const startDate = new Date(bookingData.startDate);
  const endDate = new Date(bookingData.endDate);
  document.getElementById('startDate').value = formatDateForInput(startDate);
  document.getElementById('endDate').value = formatDateForInput(endDate);

  // Crane information (using data directly from the booking)
  document.getElementById('crane').value = bookingData.craneCode || '';
  document.getElementById('location').value = bookingData.location || '';

  // Job description
  document.getElementById('description').value = bookingData.description || '';

  // Generate shift table
  generateShiftTable(bookingData);

  // Populate items
  populateItems(bookingData.items || []);

  // Populate hazards
  populateHazards(bookingData.selectedHazards || [], bookingData.customHazard);
}

// Generate shift table
function generateShiftTable(bookingData) {
  if (!bookingData || !bookingData.shifts || bookingData.shifts.length === 0) {
    shiftTableContainer.innerHTML =
      '<div class="p-3"><p class="text-muted mb-0">No shift information available</p></div>';
    return;
  }

  // Group shifts by date
  const shiftsByDate = {};

  bookingData.shifts.forEach(shift => {
    const dateStr = formatDateForInput(new Date(shift.date));

    if (!shiftsByDate[dateStr]) {
      shiftsByDate[dateStr] = [];
    }

    shiftsByDate[dateStr].push(shift);
  });

  // Get all unique shift definitions
  const shiftDefinitions = [];
  const shiftDefinitionIds = new Set();

  bookingData.shifts.forEach(shift => {
    if (!shiftDefinitionIds.has(shift.shiftDefinitionId)) {
      shiftDefinitionIds.add(shift.shiftDefinitionId);
      shiftDefinitions.push({
        id: shift.shiftDefinitionId,
        name: shift.shiftName,
        startTime: shift.startTime,
        endTime: shift.endTime
      });
    }
  });

  // Sort shift definitions by start time
  shiftDefinitions.sort((a, b) => {
    const timeA = a.startTime.split(':').map(Number);
    const timeB = b.startTime.split(':').map(Number);
    return timeA[0] * 60 + timeA[1] - (timeB[0] * 60 + timeB[1]);
  });

  // Create table HTML
  let tableHtml = `
    <table class="table table-hover mb-0">
      <thead class="table-border-top-0">
        <tr>
          <th>Date</th>
  `;

  // Add column headers for each shift definition
  shiftDefinitions.forEach(shift => {
    tableHtml += `<th>${shift.name}</th>`;
  });

  tableHtml += `
        </tr>
      </thead>
      <tbody class="table-border-bottom-0">
  `;

  // Create array of dates in the range
  const startDate = new Date(bookingData.startDate);
  const endDate = new Date(bookingData.endDate);
  const dateArray = [];

  let currentDate = new Date(startDate);
  while (currentDate <= endDate) {
    dateArray.push(new Date(currentDate));
    currentDate.setDate(currentDate.getDate() + 1);
  }

  // Process each date
  dateArray.forEach(date => {
    const dateStr = formatDateForInput(date);
    const shiftsForDate = shiftsByDate[dateStr] || [];
    const selectedShiftIds = shiftsForDate.map(s => s.shiftDefinitionId);

    const formattedDate = formatDate(date);

    tableHtml += `
      <tr>
        <td>${formattedDate}</td>
    `;

    // Add cells for each shift definition
    shiftDefinitions.forEach(shift => {
      const isSelected = selectedShiftIds.includes(shift.id);

      tableHtml += `
        <td>
          <span class="shift-indicator ${isSelected ? 'checked' : 'unchecked'}">
            ${isSelected ? '✓' : ''}
          </span>
        </td>
      `;
    });

    tableHtml += `</tr>`;
  });

  tableHtml += `
      </tbody>
    </table>
  `;

  shiftTableContainer.innerHTML = tableHtml;
}

// Populate items to be lifted
function populateItems(items) {
  if (!items || items.length === 0) {
    liftedItemsBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No items specified</td></tr>';
    return;
  }

  liftedItemsBody.innerHTML = '';

  items.forEach(item => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td>${item.itemName || ''}</td>
      <td>${item.height || ''}</td>
      <td>${item.weight || ''}</td>
      <td>${item.quantity || ''}</td>
    `;
    liftedItemsBody.appendChild(row);
  });
}

// Populate hazards
function populateHazards(selectedHazards, customHazard) {
  if ((!selectedHazards || selectedHazards.length === 0) && !customHazard) {
    hazardsContainer.innerHTML = '<div class="col-12"><p class="text-muted">No hazards specified</p></div>';
    customHazardContainer.style.display = 'none';
    return;
  }

  hazardsContainer.innerHTML = '';

  // Selected hazards
  if (selectedHazards && selectedHazards.length > 0) {
    selectedHazards.forEach(hazard => {
      const col = document.createElement('div');
      col.className = 'col-md-4 mb-2';
      col.innerHTML = `<span class="hazard-badge">${hazard.name}</span>`;
      hazardsContainer.appendChild(col);
    });
  } else {
    hazardsContainer.innerHTML = '<div class="col-12"><p class="text-muted">No hazards selected</p></div>';
  }

  // Custom hazard
  if (customHazard) {
    customHazardText.textContent = customHazard;
    customHazardContainer.style.display = 'block';
  } else {
    customHazardContainer.style.display = 'none';
  }
}

// Format date for display (DD-MM-YYYY)
function formatDate(date) {
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  return `${day}-${month}-${year}`;
}

// Format date for input fields (YYYY-MM-DD)
function formatDateForInput(date) {
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  return `${year}-${month}-${day}`;
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  bookingDetailsForm.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showForm() {
  loadingIndicator.style.display = 'none';
  bookingDetailsForm.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  bookingDetailsForm.style.display = 'none';
  errorMessage.style.display = 'block';
}
