FROM php:8.3-alpine

ENV PATH="/root/.composer/vendor/bin:${PATH}" \
    COMPOSER_ALLOW_SUPERUSER=1

RUN apk --no-cache add \
    git \
    curl \
    curl-dev \
    libcurl \
    zlib-dev \
    freetype-dev \
    jpeg-dev \
    libjpeg-turbo-dev \
    postgresql-dev \
    libpng-dev \
    icu-dev \
    gettext-dev \
    vim \
    libxml2-dev \
    freetype-dev \
    libzip-dev \
    unzip \
    libc6-compat \
    openssl \
    gcc \
    autoconf \
    make \
    libc-dev

RUN docker-php-ext-configure gd --with-freetype --with-jpeg

# Install useful extensions that are not already built into php:8.3-alpine.
# (ctype/curl/dom/iconv/fileinfo/pdo/phar/simplexml/json are bundled with
# the official image; xmlrpc and mcrypt were removed from PHP 8 core and their
# PECL ports are unmaintained, so they are no longer included.)
RUN docker-php-ext-install \
    opcache \
    bcmath \
    ftp \
    gd \
    gettext \
    intl \
    mysqli \
    pgsql \
    pcntl \
    pdo_mysql \
    pdo_pgsql \
    zip

RUN curl -sS https://getcomposer.org/installer | php -- --install-dir=/usr/local/bin --filename=composer

COPY . /app
WORKDIR /app

RUN composer install

EXPOSE 8888

ENTRYPOINT ["php"]
CMD ["server.php"]
