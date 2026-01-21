use lib::relative 'lib/perl5','lib/perl5/x86_64-linux-gnu';
use MIME::Base64;

package App::Fission::Perl;
use utf8;
use strict;
use warnings;
use Dancer2;

return sub {
  return(encode_base64('Aladdin:open sesame'));  
};


