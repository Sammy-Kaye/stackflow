// hooks.ts
// Typed Redux hooks for the StackFlow store.
//
// WHY typed hooks: The plain useSelector and useDispatch from react-redux are
// untyped by default. These wrappers bind them to RootState and AppDispatch so
// every consumer gets full TypeScript inference without importing the store types
// themselves.
//
// Usage:
//   const dispatch = useAppDispatch();
//   const { accessToken } = useAppSelector(selectAuth);

import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from './store';

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector = <T>(selector: (state: RootState) => T): T =>
  useSelector(selector);
