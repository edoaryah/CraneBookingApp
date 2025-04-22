// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const maintenanceTableContainer = document.getElementById('maintenanceTableContainer');
const maintenanceTableBody = document.getElementById('maintenanceTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Global variables
let maintenances = [];
let cranes = [];

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
  loadMaintenanceHistory();
});

// Function to load maintenance history
async function loadMaintenanceHistory() {
  showLoading();

  try {
    // Fetch both maintenances and cranes in parallel
    const [maintenancesResponse, cranesResponse] = await Promise.all([
      fetch('/api/MaintenanceSchedules'),
      fetch('/api/Cranes')
    ]);

    if (!maintenancesResponse.ok || !cranesResponse.ok) {
      throw new Error('Failed to fetch data');
    }

    maintenances = await maintenancesResponse.json();
    cranes = await cranesResponse.json();

    // Map cranes by ID for easier lookup
    const craneMap = {};
    cranes.forEach(crane => {
      craneMap[crane.id] = crane;
    });

    // Sort maintenances by start date (newest first)
    maintenances.sort((a, b) => new Date(b.startDate) - new Date(a.startDate));

    if (maintenances.length === 0) {
      showNoData();
      return;
    }

    // Render the table
    maintenanceTableBody.innerHTML = '';

    maintenances.forEach(maintenance => {
      const row = document.createElement('tr');

      // Format dates
      const startDate = new Date(maintenance.startDate);
      const endDate = new Date(maintenance.endDate);
      const formattedStartDate = formatDate(startDate);
      const formattedEndDate = formatDate(endDate);
      const dateRange = `${formattedStartDate} - ${formattedEndDate}`;

      // Get crane code
      const craneCode = maintenance.craneCode || 'Unknown';

      // Populate row
      row.innerHTML = `
        <td>${maintenance.id}</td>
        <td>${maintenance.title}</td>
        <td>${maintenance.createdBy}</td>
        <td>${dateRange}</td>
        <td>${craneCode}</td>
        <td>
          <div class="dropdown">
            <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
              <i class="bx bx-dots-vertical-rounded"></i>
            </button>
            <div class="dropdown-menu">
              <a class="dropdown-item" href="/MaintenanceHistory/Details/${maintenance.id}">
                <i class="bx bx-show-alt me-1"></i> View Details
              </a>
              <a class="dropdown-item" href="javascript:void(0);" onclick="event.stopPropagation();">
                <i class="bx bx-edit-alt me-1"></i> Edit
              </a>
              <a class="dropdown-item" href="javascript:void(0);" onclick="event.stopPropagation();">
                <i class="bx bx-trash me-1"></i> Delete
              </a>
            </div>
          </div>
        </td>
      `;

      // Add click event to the entire row
      row.style.cursor = 'pointer';
      row.addEventListener('click', function (e) {
        // Only navigate if we didn't click inside the dropdown or dropdown toggle button
        if (
          !e.target.closest('.dropdown') &&
          !e.target.closest('.dropdown-menu') &&
          !e.target.closest('.dropdown-toggle')
        ) {
          window.location.href = `/MaintenanceHistory/Details/${maintenance.id}`;
        }
      });

      maintenanceTableBody.appendChild(row);
    });

    showTable();
  } catch (error) {
    console.error('Error loading maintenance history:', error);
    showError();
  }
}

// Format date to DD-MM-YYYY
function formatDate(date) {
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  return `${day}-${month}-${year}`;
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  maintenanceTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  maintenanceTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  maintenanceTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  maintenanceTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
