# Authentication API Guide

## Overview

The RoadStall API now includes user authentication with login and registration endpoints.

## User Status Codes

- **0** = Inactive (newly registered users)
- **1** = Active (can login)
- **2** = Suspended (optional future use)

## API Endpoints

### 1. Register a New User

**Endpoint:** `POST /api/Auth/register`

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePass123!",
  "email": "john@example.com",
  "phone": "0123456789"
}
```

**Response (201 Created):**
```json
{
  "userId": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "phone": "0123456789",
  "status": 0,
  "token": "MTpqb2huX2RvZToxNzM4NzU2ODAwMDAwMDAwMDA="
}
```

**Notes:**
- Password is automatically hashed using SHA256
- New users have `status = 0` (inactive) by default
- Admin must change status to `1` to allow login
- Returns a token for immediate use if needed

---

### 2. Login

**Endpoint:** `POST /api/Auth/login`

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "SecurePass123!"
}
```

**Success Response (200 OK):**
```json
{
  "userId": 1,
  "username": "john_doe",
  "email": "john@example.com",
  "phone": "0123456789",
  "status": 1,
  "token": "MTpqb2huX2RvZToxNzM4NzU2ODAwMDAwMDAwMDA="
}
```

**Error Responses:**

**Invalid Credentials (401 Unauthorized):**
```json
{
  "message": "Invalid username or password"
}
```

**Account Inactive (401 Unauthorized):**
```json
{
  "message": "Account is not active. Please contact administrator."
}
```

---

### 3. Activate a User (Admin)

**Endpoint:** `PUT /api/Users/{id}`

**Request Body:**
```json
{
  "id": 1,
  "username": "john_doe",
  "passwordHash": "existing_hash_do_not_change",
  "email": "john@example.com",
  "phone": "0123456789",
  "status": 1
}
```

**Note:** To activate a user, change `status` from `0` to `1`.

---

## How It Works

### Registration Flow:
1. User submits registration form with plain text password
2. Backend hashes password using SHA256
3. User is saved with `status = 0` (inactive)
4. Admin manually activates user by setting `status = 1`

### Login Flow:
1. User submits username and password
2. Backend finds user by username
3. Backend hashes submitted password and compares with stored hash
4. If match and `status = 1`, login succeeds
5. Backend returns user info + token

### Password Security:
- Passwords are **never stored in plain text**
- Uses SHA256 hashing algorithm
- Passwords are hashed before saving to database
- Login verification hashes input and compares hashes

---

## Testing in Swagger

### Step 1: Register a User
1. Go to `POST /api/Auth/register`
2. Click "Try it out"
3. Use this JSON:
```json
{
  "username": "testuser",
  "password": "Test123!",
  "email": "test@example.com",
  "phone": "0123456789"
}
```
4. Execute - note the `userId` in response

### Step 2: Activate the User
1. Go to `PUT /api/Users/{id}`
2. Use the `userId` from step 1
3. Get the user first with `GET /api/Users/{id}`
4. Copy the response and change `status` to `1`
5. Execute the PUT request

### Step 3: Login
1. Go to `POST /api/Auth/login`
2. Use:
```json
{
  "username": "testuser",
  "password": "Test123!"
}
```
3. Execute - you should get user info + token

---

## Integration with Angular Frontend

### Login Service Example:

```typescript
// login.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  userId: number;
  username: string;
  email: string;
  phone?: string;
  status: number;
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://your-api-url.azurewebsites.net/api/Auth';

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials);
  }

  register(userData: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/register`, userData);
  }

  saveToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  logout(): void {
    localStorage.removeItem('authToken');
  }
}
```

### Login Component Example:

```typescript
// login.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-login',
  template: `
    <form (ngSubmit)="onLogin()">
      <input [(ngModel)]="username" name="username" placeholder="Username" required>
      <input [(ngModel)]="password" name="password" type="password" placeholder="Password" required>
      <button type="submit">Login</button>
      <div *ngIf="errorMessage" class="error">{{ errorMessage }}</div>
    </form>
  `
})
export class LoginComponent {
  username = '';
  password = '';
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onLogin(): void {
    this.authService.login({ username: this.username, password: this.password })
      .subscribe({
        next: (response) => {
          this.authService.saveToken(response.token);
          localStorage.setItem('userId', response.userId.toString());
          localStorage.setItem('username', response.username);
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.errorMessage = error.error.message || 'Login failed';
        }
      });
  }
}
```

---

## Security Notes

?? **Current Implementation:**
- Uses SHA256 for password hashing (basic security)
- Token generation is simple (not JWT)

? **For Production, Consider:**
- Upgrading to BCrypt or Argon2 for password hashing
- Implementing JWT (JSON Web Tokens) with expiration
- Adding refresh token mechanism
- Implementing rate limiting for login attempts
- Adding email verification
- Adding password reset functionality
- Using HTTPS only (already enabled in production)

---

## Common Scenarios

### Scenario 1: First Admin User
```
1. Register first user via /api/Auth/register
2. Manually activate in database: UPDATE User SET Status = 1 WHERE Id = 1
3. Login with that user
4. That user can now activate other users via /api/Users/{id}
```

### Scenario 2: User Can't Login
**Check:**
- Is `status = 1`? (Use `GET /api/Users/{id}`)
- Is username correct? (case-sensitive)
- Is password correct? (remember what you registered with)

### Scenario 3: Password Reset
**Currently not implemented. Manual workaround:**
1. Admin updates user's `passwordHash` with a new hash
2. Tell user the new temporary password
3. User logs in and changes password (need to implement password change endpoint)

---

## Summary

? **Registration:** Users can self-register (status = 0)  
? **Activation:** Admin sets status = 1 to activate  
? **Login:** Returns user info + token  
? **Security:** Passwords hashed, never stored in plain text  
? **Ready for Frontend:** Easy integration with Angular  

