import { EditingData } from '@sitecore-jss/sitecore-jss-nextjs/editing';
import { createClient, RedisClientType } from 'redis';

export interface EditingDataCache {
  set(key: string, editingData: EditingData): Promise<void>;
  get(key: string): Promise<EditingData | undefined>;
}

export class EditingDataCache implements EditingDataCache {
  protected redisClient: RedisClientType;
  private defaultTtl = 120;

  constructor(redisHost: string | undefined, redisPassword: string | undefined) {
    if (!redisHost || !redisPassword) {
      throw Error('Host || password are missing');
    }

    const [host, port] = redisHost.split(':');

    this.redisClient = createClient({
      password: redisPassword,
      socket: {
        host,
        port: parseInt(port),
        // tls: true,
        connectTimeout: 30000,
      },
    });

    this.redisClient.on('error', (params) => console.log('Redis Client error event', params));
    this.redisClient.on('connect', () => console.log('Redis Client connect event'));
    this.redisClient.on('ready', () => console.log('Redis Client ready event'));
    this.redisClient.on('end', () => console.log('Redis Client end event'));
    this.redisClient.on('reconnecting', () => console.log('Redis Client reconnecting event'));

    this.redisClient.connect();
  }

  async set(key: string, editingData: EditingData): Promise<void> {
    console.log(
      `Putting editing data for ${key} into Redis storage...`,
      process.env.REDIS_DB_HOST,
      process.env.NEXT_PUBLIC_ENV
    );
    try {
      await this.redisClient.set(key, JSON.stringify(editingData), { EX: this.defaultTtl });
    } catch (err) {
      console.error('Error setting Redis value:', err);
      throw err;
    }
  }

  async get(key: string): Promise<EditingData | undefined> {
    console.log(`Getting editing data for ${key} from Redis storage...`);
    try {
      const entry = await this.redisClient.get(key) as any;
      if (entry) {
        const result = JSON.parse(entry) as EditingData;
        await this.redisClient.expire(key, 0); // Remove the key after retrieving the value
        return result;
      }
      return undefined;
    } catch (err) {
      console.error('Error getting Redis value:', err);
      throw err;
    }
  }
}

export const editingDataCache = new EditingDataCache(
  process.env.REDIS_DB_HOST,
  process.env.REDIS_DB_PASSWORD
);
