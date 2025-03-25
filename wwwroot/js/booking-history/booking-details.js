// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const errorMessage = document.getElementById('errorMessage');
const bookingDetailsForm = document.getElementById('bookingDetailsForm');
const bookingNumber = document.getElementById('bookingNumber');
const shiftTableContainer = document.getElementById('shiftTableContainer');
const liftedItemsBody = document.getElementById('liftedItemsBody');
const hazardsContainer = document.getElementById('hazardsContainer');

// Global variables
let bookingData = null;
let craneData = null;
let hazardsData = [];

// Load booking details
async function loadBookingDetails(id) {
  showLoading();

  try {
    // Fetch booking details, crane data, and hazards in parallel
    const [bookingResponse, cranesResponse, hazardsResponse] = await Promise.all([
      fetch(`/api/Bookings/${id}`),
      fetch('/api/Cranes'),
      fetch('/api/Hazards')
    ]);

    if (!bookingResponse.ok || !cranesResponse.ok || !hazardsResponse.ok) {
      throw new Error('Failed to fetch data');
    }

    // Process the responses
    bookingData = await bookingResponse.json();
    const cranes = await cranesResponse.json();
    hazardsData = await hazardsResponse.json();

    // Find the crane for this booking
    craneData = cranes.find(c => c.id === bookingData.craneId);

    // Populate the form
    populateBookingDetails();

    // Show the form
    showForm();
  } catch (error) {
    console.error('Error loading booking details:', error);
    showError();
  }
}

// Populate booking details in the form
function populateBookingDetails() {
  if (!bookingData) return;

  // Set booking number
  bookingNumber.textContent = bookingData.bookingNumber;

  // Basic information
  document.getElementById('name').value = bookingData.name;
  document.getElementById('department').value = bookingData.department;
  document.getElementById('projectSupervisor').value = bookingData.projectSupervisor || '';
  document.getElementById('phoneNumber').value = bookingData.phoneNumber || '';
  document.getElementById('costCode').value = bookingData.costCode || '';

  // Format dates for input fields (YYYY-MM-DD)
  const startDate = new Date(bookingData.startDate);
  const endDate = new Date(bookingData.endDate);
  document.getElementById('startDate').value = formatDateForInput(startDate);
  document.getElementById('endDate').value = formatDateForInput(endDate);

  // Crane information
  document.getElementById('crane').value = craneData ? `${craneData.code} (${craneData.capacity} ton)` : 'Unknown';
  document.getElementById('location').value = bookingData.location || '';

  // Job description
  document.getElementById('description').value = bookingData.description || '';

  // Generate shift table
  generateShiftTable();

  // Populate items
  populateItems();

  // Populate hazards
  populateHazards();
}

// Generate shift table
function generateShiftTable() {
  if (!bookingData || !bookingData.shifts || bookingData.shifts.length === 0) {
    shiftTableContainer.innerHTML = '<p class="text-muted">No shift information available</p>';
    return;
  }

  // Create date range
  const startDate = new Date(bookingData.startDate);
  const endDate = new Date(bookingData.endDate);
  const dateArray = [];

  // Create array of dates in the range
  let currentDate = new Date(startDate);
  while (currentDate <= endDate) {
    dateArray.push(new Date(currentDate));
    currentDate.setDate(currentDate.getDate() + 1);
  }

  // Create table HTML
  let tableHtml = `
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

  // Process each date
  dateArray.forEach(date => {
    const dateString = formatDateForInput(date);
    const shift = bookingData.shifts.find(s => formatDateForInput(new Date(s.date)) === dateString);

    const dayShiftChecked = shift && shift.isDayShift;
    const nightShiftChecked = shift && shift.isNightShift;

    const formattedDate = formatDate(date);

    tableHtml += `
      <tr>
        <td>${formattedDate}</td>
        <td>
          <span class="shift-indicator ${dayShiftChecked ? 'checked' : 'unchecked'}">
            ${dayShiftChecked ? '✓' : ''}
          </span>
        </td>
        <td>
          <span class="shift-indicator ${nightShiftChecked ? 'checked' : 'unchecked'}">
            ${nightShiftChecked ? '✓' : ''}
          </span>
        </td>
      </tr>
    `;
  });

  tableHtml += `
      </tbody>
    </table>
  `;

  shiftTableContainer.innerHTML = tableHtml;
}

// Populate items to be lifted
function populateItems() {
  if (!bookingData || !bookingData.items || bookingData.items.length === 0) {
    liftedItemsBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No items specified</td></tr>';
    return;
  }

  liftedItemsBody.innerHTML = '';

  bookingData.items.forEach(item => {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td>${item.itemName}</td>
      <td>${item.height}</td>
      <td>${item.weight}</td>
      <td>${item.quantity}</td>
    `;
    liftedItemsBody.appendChild(row);
  });
}

// Populate hazards
function populateHazards() {
  if ((!bookingData.selectedHazards || bookingData.selectedHazards.length === 0) &&
      !bookingData.customHazard) {
    hazardsContainer.innerHTML = '<p class="text-muted">No hazards specified</p>';
    return;
  }

  let hazardsHtml = '';

  // Selected hazards
  if (bookingData.selectedHazards && bookingData.selectedHazards.length > 0) {
    bookingData.selectedHazards.forEach(hazard => {
      hazardsHtml += `<span class="hazard-badge">${hazard.name}</span>`;
    });
  }

  // Custom hazard
  if (bookingData.customHazard) {
    hazardsHtml += `
      <div class="mt-2">
        <strong>Custom Hazard:</strong> ${bookingData.customHazard}
      </div>
    `;
  }

  hazardsContainer.innerHTML = hazardsHtml;
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
