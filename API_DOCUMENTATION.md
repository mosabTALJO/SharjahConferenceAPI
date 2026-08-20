# Sharjah Conference API Documentation

This document contains the complete specification of all API endpoints, authentication flows, role-based access controls, request/response models (DTOs), enums, query parameters, and TypeScript interfaces for frontend integration.

---

## 🌐 1. General API Information

- **Base URL (Local HTTP):** `http://localhost:5041`
- **Base URL (Local HTTPS):** `https://localhost:7136`
- **Interactive Documentation (Scalar):** `http://localhost:5041/scalar/v1` (active in Development mode)
- **OpenAPI JSON Spec:** `http://localhost:5041/openapi/v1.json`
- **Authentication:** JSON Web Token (JWT) Bearer Authentication
- **Default Headers:**
  ```http
  Content-Type: application/json
  Accept: application/json
  Authorization: Bearer <your_jwt_token>
  ```

---

## 🔐 2. Roles and Permission Matrix

The platform implements three roles:

| Role | Description |
| :--- | :--- |
| **Admin** | Full system control (CRUD on Venues, Zones, Facilities, Announcements, Tables, Publishers, Participants, and Meetings). Registers publisher and admin accounts. |
| **Publisher** | Can reserve available tables, release own tables, view publisher directories, and schedule/manage own meetings. |
| **Participant** | Can view venues, zones, tables, and publisher directories. Can book meetings on tables reserved by publishers, and view/manage own meetings. |

### Endpoint Access Matrix

| Resource / Action | Admin | Publisher | Participant | Anonymous |
| :--- | :--- | :--- | :--- | :--- |
| `POST /api/Auth/login` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/Auth/register-participant` | ✅ | ✅ | ✅ | ✅ |
| `POST /api/Auth/register-publisher` | ✅ | ❌ | ❌ | ❌ |
| `POST /api/Auth/register-admin` | ✅ | ❌ | ❌ | ❌ |
| `GET /api/Auth/me` | ✅ | ✅ | ✅ | ❌ |
| `GET /api/Venues`, `GET /api/Zones` | ✅ | ✅ | ✅ | ❌ |
| `POST/PUT/DELETE /api/Venues`, `/api/Zones` | ✅ | ❌ | ❌ | ❌ |
| `GET /api/MeetingTables` | ✅ | ✅ | ✅ | ❌ |
| `POST/PUT/DELETE /api/MeetingTables` | ✅ | ❌ | ❌ | ❌ |
| `POST /api/MeetingTables/{id}/reserve` | ✅ (Any publisher) | ✅ (Self) | ❌ | ❌ |
| `POST /api/MeetingTables/{id}/unreserve` | ✅ | ✅ (Own table) | ❌ | ❌ |
| `GET /api/Publishers`, `GET /api/Publishers/{id}/tables` | ✅ | ✅ | ✅ | ❌ |
| `POST/PUT/DELETE /api/Publishers` | ✅ | ❌ | ❌ | ❌ |
| `GET /api/Participants` | ✅ | ❌ | ❌ | ❌ |
| `GET/PUT /api/Participants/{id}` | ✅ | ❌ | ✅ (Own profile) | ❌ |
| `DELETE /api/Participants/{id}` | ✅ | ❌ | ❌ | ❌ |
| `GET /api/Meetings` | ✅ (All) | ✅ (Own meetings) | ✅ (Own meetings) | ❌ |
| `POST /api/Meetings` | ✅ | ✅ | ✅ (On reserved tables) | ❌ |
| `PUT/DELETE /api/Meetings/{id}` | ✅ | ✅ (Own meetings) | ✅ (Own meetings) | ❌ |
| `GET /api/Facilities`, `GET /api/Announcements` | ✅ | ✅ | ✅ | ❌ |
| `POST/PUT/DELETE /api/Facilities`, `/api/Announcements` | ✅ | ❌ | ❌ | ❌ |
| `POST /api/Chat` | ✅ | ✅ | ✅ | ❌ |

---

## 🏷️ 3. Common Enums & Types

### `UserRole`
| Value | Number | Name | Description |
| :--- | :--- | :--- | :--- |
| `0` | 0 | `Admin` | Full administrative access |
| `1` | 1 | `Publisher` | Publisher with table reservation capabilities |
| `2` | 2 | `Participant` | Conference participant / attendee |

### `AnnouncementPriority`
| Value | Number | Name | Description |
| :--- | :--- | :--- | :--- |
| `0` | 0 | `Low` | Low priority announcement |
| `1` | 1 | `Normal` | Standard priority announcement |
| `2` | 2 | `High` | High importance announcement |
| `3` | 3 | `Urgent` | Urgent / critical announcement |

### `MeetingStatus`
| Value | Number | Name | Description |
| :--- | :--- | :--- | :--- |
| `0` | 0 | `Scheduled` | Meeting is booked and confirmed |
| `1` | 1 | `InProgress` | Meeting is currently ongoing |
| `2` | 2 | `Completed` | Meeting finished normally |
| `3` | 3 | `Cancelled` | Meeting cancelled |
| `4` | 4 | `NoShow` | Participant or publisher did not show up |

### `TableStatus`
| Value | Number | Name | Description |
| :--- | :--- | :--- | :--- |
| `0` | 0 | `Available` | Table is open for reservation |
| `1` | 1 | `Reserved` | Table is reserved by a publisher |
| `2` | 2 | `Occupied` | Table is currently occupied |

---

## 💻 4. Ready-to-use TypeScript Interfaces

```typescript
// --- Enums ---
export enum UserRole {
  Admin = 0,
  Publisher = 1,
  Participant = 2,
}

export enum AnnouncementPriority {
  Low = 0,
  Normal = 1,
  High = 2,
  Urgent = 3,
}

export enum MeetingStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
  NoShow = 4,
}

export enum TableStatus {
  Available = 0,
  Reserved = 1,
  Occupied = 2,
}

// --- Auth ---
export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponseDto {
  token: string;
  expiration: string; // ISO 8601
  role: 'Admin' | 'Publisher' | 'Participant';
  email: string;
  userId: string; // GUID
  publisherId?: string | null; // GUID
  participantId?: string | null; // GUID
}

export interface RegisterParticipantDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone: string;
  title: string;
}

export interface RegisterPublisherDto {
  nameEn: string;
  nameAr: string;
  countryCode: string;
  email: string;
  password: string;
}

export interface RegisterAdminDto {
  email: string;
  password: string;
}

export interface CurrentUserDto {
  userId: string; // GUID
  email: string;
  role: 'Admin' | 'Publisher' | 'Participant';
  publisherId?: string | null; // GUID
  participantId?: string | null; // GUID
  displayName?: string | null;
}

// --- Venues ---
export interface VenueDto {
  id: string; // GUID
  nameEn: string;
  nameAr: string;
}

export interface CreateVenueDto {
  nameEn: string;
  nameAr: string;
}

export interface UpdateVenueDto {
  nameEn: string;
  nameAr: string;
}

// --- Zones ---
export interface ZoneDto {
  id: string; // GUID
  venueId: string; // GUID
  code: string;
  nameEn: string;
  nameAr: string;
  svgBounds: string;
}

export interface CreateZoneDto {
  venueId: string; // GUID
  code: string;
  nameEn: string;
  nameAr: string;
  svgBounds: string;
}

export interface UpdateZoneDto {
  code: string;
  nameEn: string;
  nameAr: string;
  svgBounds: string;
}

// --- Meeting Tables ---
export interface MeetingTableDto {
  id: string; // GUID
  zoneId: string; // GUID
  tableNumber: string;
  coordX: number;
  coordY: number;
  capacity: number;
  operationalStatus: TableStatus;
  reservedByPublisherId?: string | null; // GUID
  publisherNameEn?: string | null;
}

export interface CreateMeetingTableDto {
  zoneId: string; // GUID
  tableNumber: string;
  coordX: number;
  coordY: number;
  capacity: number;
}

export interface UpdateMeetingTableDto {
  tableNumber: string;
  coordX: number;
  coordY: number;
  capacity: number;
  operationalStatus: TableStatus;
}

export interface ReserveTableDto {
  publisherId?: string | null; // GUID (Optional, only for Admin reserving for publisher)
}

// --- Publishers ---
export interface PublisherDto {
  id: string; // GUID
  nameEn: string;
  nameAr: string;
  countryCode: string;
}

export interface CreatePublisherDto {
  nameEn: string;
  nameAr: string;
  countryCode: string;
}

export interface UpdatePublisherDto {
  nameEn: string;
  nameAr: string;
  countryCode: string;
}

// --- Participants ---
export interface ParticipantDto {
  id: string; // GUID
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  title: string;
}

export interface CreateParticipantDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  title: string;
}

export interface UpdateParticipantDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  title: string;
}

// --- Meetings ---
export interface MeetingDto {
  id: string; // GUID
  meetingTableId: string; // GUID
  publisherId: string; // GUID
  participantId: string; // GUID
  startTime: string; // ISO 8601 DateTime
  endTime: string;   // ISO 8601 DateTime
  status: MeetingStatus;
  notes?: string | null;
}

export interface CreateMeetingDto {
  meetingTableId: string; // GUID
  publisherId: string; // GUID
  participantId: string; // GUID (Auto-filled from token for Participant role)
  startTime: string; // ISO 8601 DateTime
  endTime: string;   // ISO 8601 DateTime
  notes?: string | null;
}

export interface UpdateMeetingDto {
  startTime: string; // ISO 8601 DateTime
  endTime: string;   // ISO 8601 DateTime
  status: MeetingStatus;
  notes?: string | null;
}

// --- Facilities ---
export interface FacilityDto {
  id: string; // GUID
  category: string;
  name: string;
  coordX: number;
  coordY: number;
}

export interface CreateFacilityDto {
  category: string;
  name: string;
  coordX: number;
  coordY: number;
}

export interface UpdateFacilityDto {
  category: string;
  name: string;
  coordX: number;
  coordY: number;
}

// --- Announcements ---
export interface AnnouncementDto {
  id: string; // GUID
  title: string;
  body: string;
  priority: AnnouncementPriority;
  activeStartTime: string; // ISO 8601 DateTime
  activeEndTime: string;   // ISO 8601 DateTime
}

export interface CreateAnnouncementDto {
  title: string;
  body: string;
  priority: AnnouncementPriority;
  activeStartTime: string; // ISO 8601 DateTime
  activeEndTime: string;   // ISO 8601 DateTime
}

export interface UpdateAnnouncementDto {
  title: string;
  body: string;
  priority: AnnouncementPriority;
  activeStartTime: string; // ISO 8601 DateTime
  activeEndTime: string;   // ISO 8601 DateTime
}

// --- AI Chatbot ---
export interface ChatMessageDto {
  role: 'user' | 'assistant' | 'system';
  content: string;
}

export interface ChatRequestDto {
  message: string;
  conversationHistory?: ChatMessageDto[] | null;
  userLanguage?: string | null; // e.g. 'en', 'ar'
}

export interface ChatResponseDto {
  reply: string;
  toolCallsExecuted?: string[] | null;
}
```

---

## 📡 5. Detailed Endpoints

---

### 0. Authentication (`/api/Auth`)

#### `POST /api/Auth/login`
- **Access:** Anonymous
- **Description:** Authenticate user credentials and receive a signed JWT token.
- **Request Body:**
  ```json
  {
    "email": "admin@sharjahconference.ae",
    "password": "Admin@123456"
  }
  ```
- **Response:** `200 OK`
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "expiration": "2026-08-16T11:45:00Z",
    "role": "Admin",
    "email": "admin@sharjahconference.ae",
    "userId": "0c7c848d-cf4e-4ef9-ac53-c1584bf08699",
    "publisherId": null,
    "participantId": null
  }
  ```

#### `POST /api/Auth/register-participant`
- **Access:** Anonymous / Self-registration
- **Description:** Register a new participant account.
- **Request Body:**
  ```json
  {
    "firstName": "Salim",
    "lastName": "Al-Nuaimi",
    "email": "salim@example.com",
    "password": "Participant@123",
    "phone": "+971501234567",
    "title": "Acquisitions Director"
  }
  ```
- **Response:** `200 OK` with `AuthResponseDto`.

#### `POST /api/Auth/register-publisher`
- **Access:** `Admin` only
- **Description:** Register a new publisher organization and assign credentials.
- **Request Body:**
  ```json
  {
    "nameEn": "Kalimat Publishing",
    "nameAr": "مجموعة كلمات",
    "countryCode": "UAE",
    "email": "contact@kalimat.ae",
    "password": "Publisher@123"
  }
  ```
- **Response:** `200 OK` with `AuthResponseDto`.

#### `POST /api/Auth/register-admin`
- **Access:** `Admin` only
- **Description:** Create an additional administrator account.
- **Request Body:**
  ```json
  {
    "email": "newadmin@sharjahconference.ae",
    "password": "Admin@654321"
  }
  ```
- **Response:** `200 OK` with `AuthResponseDto`.

#### `GET /api/Auth/me`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Retrieve details of the currently logged-in user.
- **Response:** `200 OK`
  ```json
  {
    "userId": "0c7c848d-cf4e-4ef9-ac53-c1584bf08699",
    "email": "admin@sharjahconference.ae",
    "role": "Admin",
    "publisherId": null,
    "participantId": null,
    "displayName": "Administrator"
  }
  ```

---

### 1. Venues (`/api/Venues`)

#### `GET /api/Venues`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Retrieve all conference venues.
- **Response:** `200 OK`

#### `GET /api/Venues/{id}`
- **Access:** Authenticated
- **Description:** Retrieve a single venue by ID.
- **Response:** `200 OK` or `404 Not Found`

#### `POST /api/Venues`
- **Access:** `Admin` only
- **Description:** Create a new venue.
- **Response:** `201 Created`

#### `PUT /api/Venues/{id}`
- **Access:** `Admin` only
- **Description:** Update an existing venue.
- **Response:** `200 OK` or `404 Not Found`

#### `DELETE /api/Venues/{id}`
- **Access:** `Admin` only
- **Description:** Delete a venue.
- **Response:** `204 No Content` or `404 Not Found`

---

### 2. Zones (`/api/Zones`)

#### `GET /api/Zones`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Retrieve all zones, optionally filtered by `venueId`.
- **Query Parameters:** `venueId` *(optional, GUID)*
- **Response:** `200 OK`

#### `GET /api/Zones/{id}`
- **Access:** Authenticated
- **Description:** Retrieve a specific zone.
- **Response:** `200 OK` or `404 Not Found`

#### `POST /api/Zones`
- **Access:** `Admin` only
- **Description:** Create a new zone.
- **Response:** `201 Created`

#### `PUT /api/Zones/{id}`
- **Access:** `Admin` only
- **Description:** Update a zone.
- **Response:** `200 OK` or `404 Not Found`

#### `DELETE /api/Zones/{id}`
- **Access:** `Admin` only
- **Description:** Delete a zone.
- **Response:** `204 No Content` or `404 Not Found`

---

### 3. Meeting Tables (`/api/MeetingTables`)

#### `GET /api/MeetingTables`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Retrieve all meeting tables. Can filter by `zoneId` or `publisherId`.
- **Query Parameters:**
  - `zoneId` *(optional, GUID)*
  - `publisherId` *(optional, GUID)*
- **Response:** `200 OK`
  ```json
  [
    {
      "id": "e4b23f81-5517-4562-b3fc-2c963f66af11",
      "zoneId": "7ca65e64-1234-4562-b3fc-2c963f66afa6",
      "tableNumber": "T-101",
      "coordX": 150.5,
      "coordY": 220.0,
      "capacity": 4,
      "operationalStatus": 1,
      "reservedByPublisherId": "11111111-2222-3333-4444-555555555555",
      "publisherNameEn": "Kalimat Publishing"
    }
  ]
  ```

#### `GET /api/MeetingTables/{id}`
- **Access:** Authenticated
- **Response:** `200 OK` or `404 Not Found`

#### `POST /api/MeetingTables`
- **Access:** `Admin` only
- **Description:** Add a new table to the conference layout.
- **Response:** `201 Created`

#### `PUT /api/MeetingTables/{id}`
- **Access:** `Admin` only
- **Description:** Update table geometry / capacity.
- **Response:** `200 OK` or `404 Not Found`

#### `DELETE /api/MeetingTables/{id}`
- **Access:** `Admin` only
- **Description:** Remove a table from layout.
- **Response:** `204 No Content` or `404 Not Found`

#### `POST /api/MeetingTables/{id}/reserve`
- **Access:** `Admin`, `Publisher`
- **Description:** Reserve an available table. Publishers automatically reserve for themselves; Admins can optionally specify a `publisherId` in request body.
- **Request Body (Optional for Publisher, Required for Admin):**
  ```json
  {
    "publisherId": "11111111-2222-3333-4444-555555555555"
  }
  ```
- **Response:** `200 OK` with updated `MeetingTableDto` or `400 Bad Request` if table is already reserved.

#### `POST /api/MeetingTables/{id}/unreserve`
- **Access:** `Admin`, `Publisher`
- **Description:** Release a reserved table back to `Available`. Publishers can only release tables reserved by their account.
- **Response:** `204 No Content` or `403 Forbidden` / `404 Not Found`

---

### 4. Publishers (`/api/Publishers`)

#### `GET /api/Publishers`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Retrieve list of all publishers.
- **Response:** `200 OK`

#### `GET /api/Publishers/{id}`
- **Access:** Authenticated
- **Response:** `200 OK` or `404 Not Found`

#### `GET /api/Publishers/{id}/tables`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Retrieve all tables reserved by a specific publisher.
- **Response:** `200 OK`
  ```json
  [
    {
      "id": "e4b23f81-5517-4562-b3fc-2c963f66af11",
      "zoneId": "7ca65e64-1234-4562-b3fc-2c963f66afa6",
      "tableNumber": "T-101",
      "coordX": 150.5,
      "coordY": 220.0,
      "capacity": 4,
      "operationalStatus": 1,
      "reservedByPublisherId": "11111111-2222-3333-4444-555555555555",
      "publisherNameEn": "Kalimat Publishing"
    }
  ]
  ```

#### `POST /api/Publishers`, `PUT /api/Publishers/{id}`, `DELETE /api/Publishers/{id}`
- **Access:** `Admin` only

---

### 5. Participants (`/api/Participants`)

#### `GET /api/Participants`
- **Access:** `Admin` only
- **Description:** List all registered participants.
- **Response:** `200 OK`

#### `GET /api/Participants/{id}`
- **Access:** `Admin` or Participant (own profile only)
- **Response:** `200 OK` or `403 Forbidden` / `404 Not Found`

#### `PUT /api/Participants/{id}`
- **Access:** `Admin` or Participant (own profile only)
- **Description:** Update participant details.
- **Response:** `200 OK` or `403 Forbidden`

#### `DELETE /api/Participants/{id}`
- **Access:** `Admin` only

---

### 6. Meetings (`/api/Meetings`)

#### `GET /api/Meetings`
- **Access:** Authenticated
- **Description:** Retrieve meetings. Admin sees all meetings; Publisher/Participant sees only their relevant scheduled meetings.
- **Response:** `200 OK`

#### `GET /api/Meetings/{id}`
- **Access:** `Admin`, or attendee (Publisher or Participant)
- **Response:** `200 OK` or `403 Forbidden` / `404 Not Found`

#### `POST /api/Meetings`
- **Access:** `Admin`, `Participant`, `Publisher`
- **Description:** Book a meeting.
  - **Rule:** The target `meetingTableId` must be reserved by `publisherId`.
  - **Rule:** Start time must be before End time.
  - **Rule:** Neither the table, publisher, nor participant can have an overlapping active meeting.
- **Request Body:**
  ```json
  {
    "meetingTableId": "e4b23f81-5517-4562-b3fc-2c963f66af11",
    "publisherId": "11111111-2222-3333-4444-555555555555",
    "participantId": "22222222-3333-4444-5555-666666666666",
    "startTime": "2026-11-01T10:00:00Z",
    "endTime": "2026-11-01T10:30:00Z",
    "notes": "Discuss translation rights"
  }
  ```
- **Response:** `201 Created` or `400 Bad Request`

#### `PUT /api/Meetings/{id}`
- **Access:** `Admin`, or attendee (Publisher or Participant)
- **Description:** Update meeting schedule or status.
- **Response:** `200 OK`

#### `DELETE /api/Meetings/{id}`
- **Access:** `Admin`, or attendee (Publisher or Participant)
- **Description:** Cancel meeting.
- **Response:** `204 No Content`

---

### 7. Facilities (`/api/Facilities`)

- `GET /api/Facilities` & `GET /api/Facilities/{id}`: `[Authorize]` (Admin, Publisher, Participant)
- `POST`, `PUT`, `DELETE`: `[Authorize(Roles = "Admin")]`

---

### 8. Announcements (`/api/Announcements`)

- `GET /api/Announcements` & `GET /api/Announcements/{id}`: `[Authorize]` (Admin, Publisher, Participant)
- `POST`, `PUT`, `DELETE`: `[Authorize(Roles = "Admin")]`

---

### 9. AI Chatbot (`/api/Chat`)

The conference AI Chatbot is powered by Google AI Studio (Gemini 3.5 Flash) through `Microsoft.Extensions.AI`. It supports automatic function calling against the conference database with strict grounding and bilingual Arabic/English responses.

#### `POST /api/Chat`
- **Access:** Authenticated (`Admin`, `Publisher`, `Participant`)
- **Description:** Submit a natural language question in English or Arabic (e.g. asking about table locations, publisher schedules, facilities, or announcements) and receive a synthesized, strictly-grounded response.
- **Underlying Deterministic AI Tools**:
  - `SearchDirectory`: Fuzzy search across participants and publishers by name, title, organization, or country code.
  - `GetPublisherReservedTables`: Typo-tolerant bilingual search for meeting tables reserved by a specific publisher by English or Arabic name.
  - `GetTableStatus`: Real-time status, coordinates, and upcoming meetings for a specific meeting table.
  - `GetParticipantSchedule`: Timetables and locations for individual participant meetings.
  - `GetFacilityLocation`: Coordinates and nearest zone for conference facilities.
  - `GetActiveAnnouncements`: Real-time broadcast announcements for attendees.
- **Request Headers:**
  ```http
  Authorization: Bearer <jwt_token>
  Content-Type: application/json
  ```
- **Request Body:**
  ```json
  {
    "message": "أين تقع دار الشروق وما هو رقم طاولتهم؟",
    "conversationHistory": [
      {
        "role": "user",
        "content": "مرحباً، أبحث عن دور النشر المشاركة."
      },
      {
        "role": "assistant",
        "content": "أهلاً بك في مؤتمر الشارقة للناشرين! يمكنك سؤالي عن أي دار نشر أو مشارك وسأزودك بتفاصيل موقعهم وطاولاتهم."
      }
    ],
    "userLanguage": "ar"
  }
  ```
- **Response:** `200 OK`
  ```json
  {
    "reply": "تقع دار الشروق في المنطقة B ومحجوز لها الطاولة رقم 12. إذا احتجت إلى أي مساعدة أخرى، يمكنك زيارة مكتب الاستعلامات في المنطقة A.",
    "toolCallsExecuted": null
  }
  ```
- **Error Responses:**
  - `400 Bad Request`: Message is empty or missing.
  - `401 Unauthorized`: Missing or invalid JWT Bearer token.
  - `500 Internal Server Error`: Issue connecting to LLM provider.

