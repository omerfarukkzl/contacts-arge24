import { computed, Injectable, signal } from '@angular/core';
import { FirebaseError, getApp, getApps, initializeApp } from 'firebase/app';
import {
  Auth,
  User,
  browserLocalPersistence,
  createUserWithEmailAndPassword,
  getAuth,
  getIdToken as getFirebaseIdToken,
  onAuthStateChanged,
  sendPasswordResetEmail,
  setPersistence,
  signInWithEmailAndPassword,
  signOut
} from 'firebase/auth';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly notConfiguredCode = 'auth/not-configured';
  private readonly firebaseConfig = environment.firebase;

  private auth: Auth | null = null;
  private authStateUnsubscribe: (() => void) | null = null;

  private readonly authUser = signal<User | null>(null);
  readonly initialized = signal(false);
  readonly session = computed(() => this.authUser());
  readonly isAuthenticated = computed(() => this.authUser() !== null);
  readonly isConfigured = computed(() => this.hasConfiguration());
  readonly userEmail = computed(() => this.authUser()?.email ?? null);

  async initialize(): Promise<void> {
    if (this.initialized()) {
      return;
    }

    if (!this.hasConfiguration()) {
      this.authUser.set(null);
      this.initialized.set(true);
      return;
    }

    const app = getApps().length > 0 ? getApp() : initializeApp(this.firebaseConfig);
    const auth = getAuth(app);
    this.auth = auth;

    await setPersistence(auth, browserLocalPersistence);

    await new Promise<void>((resolve) => {
      let resolved = false;

      this.authStateUnsubscribe = onAuthStateChanged(
        auth,
        (user) => {
          this.authUser.set(user);

          if (!resolved) {
            resolved = true;
            this.initialized.set(true);
            resolve();
          }
        },
        () => {
          this.authUser.set(null);

          if (!resolved) {
            resolved = true;
            this.initialized.set(true);
            resolve();
          }
        }
      );
    });
  }

  async signup(email: string, password: string): Promise<void> {
    const auth = this.requireAuth();
    const credentials = await createUserWithEmailAndPassword(auth, email, password);
    this.authUser.set(credentials.user);
  }

  async login(email: string, password: string): Promise<void> {
    const auth = this.requireAuth();
    const credentials = await signInWithEmailAndPassword(auth, email, password);
    this.authUser.set(credentials.user);
  }

  async sendPasswordReset(email: string): Promise<void> {
    const auth = this.requireAuth();
    await sendPasswordResetEmail(auth, email);
  }

  async logout(): Promise<void> {
    const auth = this.auth;
    if (!auth) {
      this.authUser.set(null);
      return;
    }

    await signOut(auth);
    this.authUser.set(null);
  }

  async getIdToken(forceRefresh = false): Promise<string | null> {
    const auth = this.auth;
    const user = auth?.currentUser ?? this.authUser();

    if (!user) {
      return null;
    }

    return getFirebaseIdToken(user, forceRefresh);
  }

  mapErrorToTranslationKey(error: unknown): string {
    const firebaseCode = this.extractFirebaseErrorCode(error);

    switch (firebaseCode) {
      case 'auth/email-already-in-use':
        return 'ERRORS.AUTH.EMAIL_EXISTS';
      case 'auth/invalid-credential':
      case 'auth/invalid-login-credentials':
      case 'auth/wrong-password':
      case 'auth/user-not-found':
      case 'auth/invalid-email':
        return 'ERRORS.AUTH.INVALID_CREDENTIALS';
      case 'auth/user-disabled':
        return 'ERRORS.AUTH.USER_DISABLED';
      case 'auth/too-many-requests':
        return 'ERRORS.AUTH.TOO_MANY_ATTEMPTS';
      case this.notConfiguredCode:
      case 'auth/operation-not-allowed':
        return 'ERRORS.AUTH.NOT_CONFIGURED';
      default:
        return 'ERRORS.REQUEST_FAILED';
    }
  }

  private requireAuth(): Auth {
    if (!this.hasConfiguration()) {
      throw new Error(this.notConfiguredCode);
    }

    if (!this.auth) {
      throw new Error(this.notConfiguredCode);
    }

    return this.auth;
  }

  private hasConfiguration(): boolean {
    return Boolean(
      this.firebaseConfig.apiKey &&
      this.firebaseConfig.projectId &&
      this.firebaseConfig.authDomain &&
      this.firebaseConfig.appId
    );
  }

  private extractFirebaseErrorCode(error: unknown): string {
    if (error instanceof FirebaseError) {
      return error.code;
    }

    if (error instanceof Error) {
      return error.message;
    }

    return 'unknown';
  }
}
