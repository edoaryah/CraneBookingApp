// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const errorMessage = document.getElementById('errorMessage');
const maintenanceDetailsForm = document.getElementById('maintenanceDetailsForm');
const maintenanceTitle = document.getElementById('maintenanceTitle');
const shiftTableContainer = document.getElementById('shiftTableContainer').querySelector('.table-responsive');

// Load maintenance details
async function loadMaintenanceDetails(id) {
  showLoading();

  try {
    // Fetch maintenance details from the API endpoint
    const maintenanceResponse = await fetch(`/api/MaintenanceSchedules/${id}`);

    if (!maintenanceResponse.ok) {
      throw new Error('Failed to fetch maintenance data');
    }

    // Process the response
    const maintenanceData = await maintenanceResponse.json();

    // Populate the form
    populateMaintenanceDetails(maintenanceData);

    // Show the form
    showForm();
  } catch (error) {
    console.error('Error loading maintenance details:', error);
    showError();
  }
}

// Populate maintenance details in the form
function populateMaintenanceDetails(maintenanceData) {
  if (!maintenanceData) return;

  // Set maintenance title
  maintenanceTitle.textContent = maintenanceData.title;

  // Basic information
  document.getElementById('createdBy').value = maintenanceData.createdBy || '';

  // Format created at date and time
  const createdAtDate = new Date(maintenanceData.createdAt);
  document.getElementById('createdAt').value = formatDateTime(createdAtDate);

  document.getElementById('title').value = maintenanceData.title || '';

  // Format dates for input fields (YYYY-MM-DD)
  const startDate = new Date(maintenanceData.startDate);
  const endDate = new Date(maintenanceData.endDate);
  document.getElementById('startDate').value = formatDateForInput(startDate);
  document.getElementById('endDate').value = formatDateForInput(endDate);

  // Crane information
  document.getElementById('crane').value = maintenanceData.craneCode || '';

  // Description
  document.getElementById('description').value = maintenanceData.description || '';

  // Generate shift table
  generateShiftTable(maintenanceData);
}

// Generate shift table
function generateShiftTable(maintenanceData) {
  if (!maintenanceData || !maintenanceData.shifts || maintenanceData.shifts.length === 0) {
    shiftTableContainer.innerHTML =
      '<div class="p-3"><p class="text-muted mb-0">No shift information available</p></div>';
    return;
  }

  // Group shifts by date
  const shiftsByDate = {};

  maintenanceData.shifts.forEach(shift => {
    const dateStr = formatDateForInput(new Date(shift.date));

    if (!shiftsByDate[dateStr]) {
      shiftsByDate[dateStr] = [];
    }

    shiftsByDate[dateStr].push(shift);
  });

  // Get all unique shift definitions
  const shiftDefinitions = [];
  const shiftDefinitionIds = new Set();

  maintenanceData.shifts.forEach(shift => {
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
  const startDate = new Date(maintenanceData.startDate);
  const endDate = new Date(maintenanceData.endDate);
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

// Format date to DD-MM-YYYY
function formatDate(date) {
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  return `${day}-${month}-${year}`;
}

// Format date and time for display
function formatDateTime(date) {
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${day}-${month}-${year} ${hours}:${minutes}`;
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
  maintenanceDetailsForm.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showForm() {
  loadingIndicator.style.display = 'none';
  maintenanceDetailsForm.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  maintenanceDetailsForm.style.display = 'none';
  errorMessage.style.display = 'block';
}
