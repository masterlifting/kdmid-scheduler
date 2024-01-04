/** @format */

import { TypedUseSelectorHook, useSelector } from 'react-redux';
import { RootState } from '../_store';

export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
