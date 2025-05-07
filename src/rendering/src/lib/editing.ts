import {
  EditingDataDiskCache,
  BasicEditingDataService,
  ServerlessEditingDataService
} from '@sitecore-jss/sitecore-jss-nextjs/editing';
import os from 'os';

export const editingDataDiskCache = new EditingDataDiskCache(os.tmpdir());

export const myEditingDataService = process.env.VERCEL
  ? new ServerlessEditingDataService()
  : new BasicEditingDataService({
      editingDataCache: editingDataDiskCache,
    });