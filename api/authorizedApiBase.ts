export class IConfig {
  token: string | undefined;
}

export class AuthorizedApiBase {
    private readonly config: IConfig;

    protected constructor(config: IConfig) {
        this.config = config;
    }

    protected transformOptions = (options: RequestInit): Promise<RequestInit> => {
        if (this.config.token) {
            options.headers = {
                ...options.headers,
                Authorization: `Bearer ${this.config.token}`,
            };
        }

        return Promise.resolve(options);
    };
}
